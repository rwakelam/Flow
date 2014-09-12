using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Timers;
using Microsoft.Win32;
using FlowLibrary;
using SynchronisationService.Configuration;
using System.IO.Abstractions;

namespace SynchronisationService
{
   
    // TODO:: rename as flow service
    // TODO:: move push code to dedicated pusher service (thereby making space for potential CleanerService)
    // TODO:: sort out file system injection
    // TODO:: sort out queue locking
    // TODO:: delete sync queue object
    // TODO:: delete redundant eventargs, exceptions, constants
    // TODO:: replace *.* with MatchAllPattern constant
    // TODO:: figure out how to unit test service
    // TODO:: fix wix service installer
    // TODO:: implement push task class? make it runnable?

    public partial class SynchronisationService : ServiceBase
    {
        // create a file watcher for each pusher config element
        // collect the paths of created/updated files
        // will need to store target paths along with sourcce ones
        // this necessitates dedicated class
        // PushQueuer? class combines watcher, queue,  pusher
        // alternatley queue could be centralised if watcher were wrapped
        // in class which generated push tasks - PushFileTaskGenerator, Watcher
        // PushMaker, TaskFactory? PushQueuer, PushDelegator, Recorder, PushArbiter
        // tasks could be delegates
        #region Constants
        
        // Configuration
        private const string ConfigurationSectionName = "SynchronisationServiceSection";

        // Messages
        private const string ConfigurationNotFoundMessage = "Configuration element not found. Name: '{0}'.";
        private const string ServiceConfiguredMessage = "Service configured. Mode: '{0}'. TimerInterval: {1}.";
        private const string SynchroniserCreatedMessage = 
            "Synchroniser created. Name: '{0}'. ClientDirectory: '{1}'. ServerDirectory: '{2}'. FilePattern: '{3}'. DeleteFromClientEnabled: '{4}'.";
        private const string SynchronsierNotCreatedMessage = "Synchronsier could not be created. Name: '{0}'. Exception: {1}.";     
        private const string SyncStartedMessage = "{0} started.";
        private const string SyncCompletedMessage = "{0} completed.";
        private const string SynchroniserStartedMessage = "Synchronsier started. Name: '{0}'.";
        private const string SynchroniserCompletedMessage = "Synchronsier completed. Name: '{0}'.";
        private const string SynchroniserFailedMessage = "Synchroniser failed. Name: '{0}'. Exception: {1}.";
        private const string PowerEventDetectedMessage = "Power event detected. Power status: '{0}'.";
        private const string TimerElapsedMessage = "Timer elapsed.";
        
        public enum Mode
        {
            //Power,
            Timer
        }
        // Power mode disabled because the "push" hooks don't work.
        // Windows doesn't wait for the service to finish before it shuts down/goes to sleep. 
        // So the push never gets completed, even though the Shutdown/Suspend event was detected successfully.

        // Defaults
        public const Mode DefaultMode = Mode.Timer;
        public const double DefaultTimerInterval = 15;

        #endregion

        #region Fields

        private Dictionary<string, PushableEventWatcher> _pushDelegators;
        //private Logger _Logger;
        private Mode _Mode = DefaultMode;
        private System.Timers.Timer _Timer;
        private Dictionary<string, string> _filePushTasks;

        #endregion

        public SynchronisationService()
        {
            InitializeComponent();
            _Timer = new System.Timers.Timer();
            _Timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
            _Timer.AutoReset = false;
        } 

        // NOTE:: For this to fire, CanHandlePowerEvent needs to be enabled on ServiceBase.
        //protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)        
        //{            
        //    eventLog.WriteEntry(String.Format(PowerEventDetectedMessage, Enum.GetName(typeof(PowerBroadcastStatus), powerStatus)), EventLogEntryType.Information);
        //    if (_Mode == Mode.Power)
        //    {
        //         switch (powerStatus)
        //        {
        //            case PowerBroadcastStatus.ResumeSuspend:
        //                Pull();
        //                break;
        //            case PowerBroadcastStatus.Suspend:
        //                Push();
        //                break;
        //        }
        //    }           
        //    return true;
        //}

        // NOTE:: For this to fire, CanHandleShutdown needs to be enabled on ServiceBase.
        //protected override void OnShutdown() 
        //{
        //    Push();
        //}

        protected override void OnStart(string[] args)
        {
            // Apply the configuration.
            ServiceSection serviceConfig = (ServiceSection)ConfigurationManager.GetSection(ConfigurationSectionName);
            if (serviceConfig == null)
            {
                throw new ConfigurationErrorsException(String.Format(ConfigurationNotFoundMessage, ConfigurationSectionName));
            }
            //_Mode = serviceConfig.Mode;
            //if (_Mode == Mode.Timer)
            //{
            //    _Timer.Interval = TimeSpan.FromMinutes(serviceConfig.TimerInterval).TotalMilliseconds;
            //}
            //string serviceConfiguredMessage = String.Format(ServiceConfiguredMessage, _Mode, serviceConfig.TimerInterval);
            //eventLog.WriteEntry(serviceConfiguredMessage, EventLogEntryType.Information);
            //SubscriptionLevel loggingLevel = (SubscriptionLevel)Enum.Parse(typeof(SubscriptionLevel), serviceConfig.LoggingLevel);

            // Create the Directory push delegators.
            _pushDelegators = new Dictionary<string, PushableEventWatcher>();
           // _Logger = new Logger(this, serviceConfig.LoggingLevel);
            if (serviceConfig.Pushers != null) 
            {
                foreach (PusherElement pusherConfig in serviceConfig.Pushers)
                {
                    try
                    {
                        PushableEventWatcher pushDelegator = new PushableEventWatcher(
                            pusherConfig.SourcePath, 
                            pusherConfig.TargetPath,
                            pusherConfig.Pattern, 
                            pusherConfig.Attributes);
                        _pushDelegators.Add(pusherConfig.Name, pushDelegator);
                        pushDelegator.OnPushable += new PushableEventWatcher.OnPushableEventHandler(OnPushRequired); 

                        //_Logger.Subscribe(pushDelegator);
                        string synchroniserCreatedMessage = String.Format(SynchroniserCreatedMessage, 
                            pusherConfig.Name, pusherConfig.TargetPath,
                            pusherConfig.SourcePath, pusherConfig.Pattern, 
                            pusherConfig.Attributes);
                        eventLog.WriteEntry(synchroniserCreatedMessage, EventLogEntryType.Information);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry(string.Format(SynchronsierNotCreatedMessage, pusherConfig.Name, ex.Message), 
                            EventLogEntryType.Error);
                    }
                }
            }
            
            // Kick off a pull, but in a new thread so that the start method can still return in a timely fashion.
           // new Thread(Pull).Start(); 

            // Start the refresh mechanism.
            switch(_Mode)
            {
                //case Mode.Power:
                //    break;
                case Mode.Timer:
                    _Timer.Start();
                    break;
            }            

        }

        private void OnPushRequired(PushableEventWatcher.PushRequiredEventArgs e)
        {
            lock (_filePushTasks)
            {
                if (_filePushTasks.ContainsKey(e.SourcePath))
                {
                    return;
                }
                _filePushTasks.Add(e.SourcePath, e.TargetPath);
            }
        }

        protected override void OnStop()
        {   
            // Stop the refresh mechanism.
            switch (_Mode)
            {
                //case Mode.Power:
                //    break;
                case Mode.Timer:
                    _Timer.Stop();
                    break;
            }  
        }

        private void OnTimerElapsed(object source, ElapsedEventArgs e)
        {
            eventLog.WriteEntry(TimerElapsedMessage, EventLogEntryType.Information);
            Push();           
            _Timer.Start();
        }
        
        private void Push()
        {
            // Run the synchronisers.

            var fileSystem = new System.IO.Abstractions.FileSystem();
            lock (_filePushTasks)
            {
                // clone list and then pass it to new thread for processing?
                foreach (KeyValuePair<string, string> kvp in _filePushTasks)
                {
                    Pusher.PushFile(fileSystem, kvp.Key, kvp.Value);
                    //_filePushTasks.Remove(kvp.Key);
                }
                _filePushTasks.Clear();
            }
            // kick off new thread which runs through list?
        }
                
    }

}
