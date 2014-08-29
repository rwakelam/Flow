using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationLibrary;
using System.Diagnostics;

namespace SynchronisationService
{
    internal class Logger : Subscriber//listener, message builder? SynchroniserLogger?
    {
        private SynchronisationService _Service;

        public Logger(SynchronisationService service, SubscriptionLevel loggingLevel) : base(loggingLevel)
        {
            _Service = service;
        }

        public override void OnError(object sender, SyncErrorEventArgs e)
        {
            _Service.eventLog.WriteEntry(e.Exception.Message, EventLogEntryType.Error);
        }
                
        public override void OnFileSyncCompleted(object sender, FileSyncEventArgs e)
        {
            string message = String.Format("File {0}. Path: '{1}'.", e.Result.ToString().ToLower(), e.Path);
            _Service.eventLog.WriteEntry(message, EventLogEntryType.Information);
        }

        public override void OnDirectorySyncCompleted(object sender, DirectorySyncEventArgs e)
        {
            IEnumerable<SyncResult> syncResults = 
                e.FileSyncResults.Select<KeyValuePair<string, FileSyncResult>, SyncResult>(x => x.Value.Result);
            Array values = Enum.GetValues(typeof(SyncResult));
            List<string> fileResults = new List<string>();
            foreach (SyncResult value in values)
            {
                int count = syncResults.Count(x => x == value);
                if (count > 0)
                {
                    fileResults.Add(String.Format("{0} {1}", count, value.ToString().ToLower()));
                }
            }
            string fileSummary = fileResults.Count > 0 ?
                String.Format("File summary: {0}.", String.Join(", ", fileResults)) : null;
            string message = String.Format("Directory synced. Path: '{0}'. {1}", e.Path, fileSummary).TrimEnd();
            Console.ForegroundColor = ConsoleColor.White;
            _Service.eventLog.WriteEntry(message, EventLogEntryType.Information);
        }

    }

}
