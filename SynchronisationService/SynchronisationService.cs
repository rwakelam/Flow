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
using WrapperLibrary;

namespace SynchronisationService
{
    // TODO:: 
    // 4. test change of event source log name.
    // streamline process so that only recently modified files are checked?
    // implement a PushNew method? PushSince?
    // synchroniser could record start time of last push itself
    // it could really do with the time of the last sync being persisted somewhere externally
    // otherwise the optimisation would only kick in second time round
    // which would defeat the object, if the first time takes hours
    // question is: where would you preserve the last sync time?
    // could you get it from the server? only if there is only one client.
    // could use a FileSystemWatcher - but again this would depend on the directories being sync-ed to start with
    // there is some scope to optimise what I've got
    // ie the 2 SynchroniseFile calls both replicate tests that have already been done at the directory level

    // what about some sort of compare the latest loop?
    // get the time of the last write on the source side
    // get the time of the last write on the target side
    // all of the source files which have been written to since the last write on the target side can go straight across
    // get the time of the oldest write on the source side
    // get the time of the oldest write on the target side
    // everything on the source side which pre-dates the earliest target side write can safely be ignored.
    // only the stuff between these two points needs to be checked against its twin

    // in practice there will be huge swathes of files with identical write times
    // if the latest write times in both directories match, chances are they are already in sync.
    // if filecount is also the same, chance increases
    // if oldest file is also the same higher still
    // these metrics would have to take account of the copyability/deletability of the files.

    // if source is more recent than target, sync source to target
    // if source is older than most recent target, check source 
    // eventually will hit 
    // 
    public partial class SynchronisationService : ServiceBase
    {

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

        private Dictionary<string, Synchroniser> _Synchronisers;
        private Logger _Logger;
        private Mode _Mode = DefaultMode;
        private System.Timers.Timer _Timer;
        private Object _Lock = new Object();

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
            _Mode = serviceConfig.Mode;
            if (_Mode == Mode.Timer)
            {
                _Timer.Interval = TimeSpan.FromMinutes(serviceConfig.TimerInterval).TotalMilliseconds;
            }
            string serviceConfiguredMessage = String.Format(ServiceConfiguredMessage, _Mode, serviceConfig.TimerInterval);
            eventLog.WriteEntry(serviceConfiguredMessage, EventLogEntryType.Information);
            //SubscriptionLevel loggingLevel = (SubscriptionLevel)Enum.Parse(typeof(SubscriptionLevel), serviceConfig.LoggingLevel);

            // Create the synchronisers.
            _Synchronisers = new Dictionary<string, Synchroniser>();
            _Logger = new Logger(this, serviceConfig.LoggingLevel);
            if (serviceConfig.Synchronisers != null) 
            {
                IFileSystemWrapper fileSystemWrapper = FileSystemWrapper.GetFileSystemWrapper();
                foreach (SynchroniserElement synchroniserConfig in serviceConfig.Synchronisers)
                {
                    try
                    {
                        Synchroniser synchroniser = new Synchroniser(fileSystemWrapper,
                            synchroniserConfig.ClientDirectory, 
                            synchroniserConfig.ServerDirectory,
                            synchroniserConfig.FilePattern, synchroniserConfig.DeleteUnmatchedEnabled);
                        _Synchronisers.Add(synchroniserConfig.Name, synchroniser);
                        _Logger.Subscribe(synchroniser);
                        string synchroniserCreatedMessage = String.Format(SynchroniserCreatedMessage, 
                            synchroniserConfig.Name, synchroniserConfig.ClientDirectory,
                            synchroniserConfig.ServerDirectory, synchroniserConfig.FilePattern, 
                            synchroniserConfig.DeleteUnmatchedEnabled);
                        eventLog.WriteEntry(synchroniserCreatedMessage, EventLogEntryType.Information);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry(string.Format(SynchronsierNotCreatedMessage, synchroniserConfig.Name, ex.Message), 
                            EventLogEntryType.Error);
                    }
                }
            }
            
            // Kick off a pull, but in a new thread so that the start method can still return in a timely fashion.
            new Thread(Pull).Start(); 

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
            Pull(); 
            _Timer.Start();
        }

        private void Pull()
        {
            // Run the synchronisers.
            lock(_Lock)
            {
                string directionName = Enum.GetName(typeof(Direction), Direction.Pull);
                eventLog.WriteEntry(string.Format(SyncStartedMessage, directionName), EventLogEntryType.Information);
                foreach (KeyValuePair<string, Synchroniser> kvp in _Synchronisers)
                {
                    try
                    {
                        eventLog.WriteEntry(String.Format(SynchroniserStartedMessage, kvp.Key), EventLogEntryType.Information);                    
                        DirectorySyncResult result = kvp.Value.Pull();
                        eventLog.WriteEntry(String.Format(SynchroniserCompletedMessage, kvp.Key), EventLogEntryType.Information);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry(String.Format(SynchroniserFailedMessage, kvp.Key, ex.Message), EventLogEntryType.Error);
                    }
                }
                eventLog.WriteEntry(string.Format(SyncCompletedMessage, directionName), EventLogEntryType.Information);
            }
        }

        private void Push()
        {
            // Run the synchronisers.
            lock (_Lock)
            {
                string directionName = Enum.GetName(typeof(Direction), Direction.Push);
                eventLog.WriteEntry(string.Format(SyncStartedMessage, directionName), EventLogEntryType.Information);
                foreach (KeyValuePair<string, Synchroniser> kvp in _Synchronisers)
                {
                    try
                    {
                        eventLog.WriteEntry(String.Format(SynchroniserStartedMessage, kvp.Key), EventLogEntryType.Information);
                        DirectorySyncResult result = kvp.Value.Push();
                        eventLog.WriteEntry(String.Format(SynchroniserCompletedMessage, kvp.Key), EventLogEntryType.Information);
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry(String.Format(SynchroniserFailedMessage, kvp.Key, ex.Message), EventLogEntryType.Error);
                    }
                }
                eventLog.WriteEntry(string.Format(SyncCompletedMessage, directionName), EventLogEntryType.Information);
            }
        }
                
    }

}
