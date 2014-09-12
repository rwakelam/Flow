using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationService.Configuration;
using FlowLibrary;

namespace SynchronisationService
{
    private class PusherService //FilePusherService?
    {
        #region Fields

        private List<PushableEventWatcher> _watchers;  
        private Dictionary<string, string> _tasks;

        #endregion 

        #region Properties

        public bool Enabled 
        {
            get 
            {
                return _watchers.Count == 0 ? false : _watchers[0].Enabled;
            }
            set 
            {
                foreach (var watcher in _watchers) 
                {
                    watcher.Enabled = value;
                }
            }
        }

        #endregion


        public PusherService(PusherCollection config)
        {
            // Create and configure the watchers.
            _watchers = new List<PushableEventWatcher>();
            foreach (PusherElement pusherConfig in config)
            {
                try
                {
                    PushableEventWatcher watcher = new PushableEventWatcher(
                        pusherConfig.SourcePath, pusherConfig.TargetPath, pusherConfig.Pattern,
                        pusherConfig.Attributes);//config more like folder mapping? would be same for cleaner
                    _watchers.Add(watcher);
                    watcher.OnPushable += new PushableEventWatcher.OnPushableEventHandler(OnPushableEvent);

                    //_Logger.Subscribe(pushDelegator);
                    //string synchroniserCreatedMessage = String.Format(SynchroniserCreatedMessage,
                    //    pusherConfig.Name, pusherConfig.TargetPath,
                    //    pusherConfig.SourcePath, pusherConfig.Pattern,
                    //    pusherConfig.Attributes);
                    //eventLog.WriteEntry(synchroniserCreatedMessage, EventLogEntryType.Information);
                    }
                catch (Exception ex)
                {
                    //eventLog.WriteEntry(string.Format(SynchronsierNotCreatedMessage, pusherConfig.Name, ex.Message),
                    //    EventLogEntryType.Error);
                }
                
            }
        }

        private void OnPushableEvent(OnPushableEventArgs e)
        {
            lock (_tasks)
            {
                if (_tasks.ContainsKey(e.SourcePath))
                {
                    return;
                }
                _tasks.Add(e.SourcePath, e.TargetPath);
            }
        }
        
        public void Push()
        {
            // Run the synchronisers.

            var fileSystem = new System.IO.Abstractions.FileSystem();
            lock (_tasks)
            {
                // clone list and then pass it to new thread for processing?
                foreach (KeyValuePair<string, string> kvp in _tasks)
                {
                    Pusher.PushFile(fileSystem, kvp.Key, kvp.Value);
                    //_filePushTasks.Remove(kvp.Key);
                }
                _tasks.Clear();
            }
            // kick off new thread which runs through list?
        }
    }
}
