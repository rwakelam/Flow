using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationLibrary;

namespace PusherConsoleApplication
{

    internal class ConsoleWriter : PusherSubscriber
    {
        private ConsoleColor GetColor(SyncResult result)
        {
            ConsoleColor color;
            switch(result)
            {
                case SyncResult.Unchanged:
                    color = ConsoleColor.White;
                    break;
                case SyncResult.Ignored:
                    color = ConsoleColor.White;
                    break;
                case SyncResult.Failed:
                    color = ConsoleColor.Red;
                    break;
                default:
                    color = ConsoleColor.Yellow;
                    break;
            }
            return color;            
        }

        //Default.
        public ConsoleWriter()
        {
        }

        public override void OnError(EntryErrorEventArgs e)
        {
            WriteError(String.Format("Error: {0}", e.Exception.Message));
        }

        public override void OnDirectoryCreated(EntryEventArgs e)
        {
            WriteWarning(String.Format("Directory created. Path: \"{0}\".", e.Path));
        }

        public override void OnDirectoryUpdated(EntryEventArgs e)
        {
            WriteWarning(String.Format("Directory updated. Path: \"{0}\".", e.Path));
        }

        public override void OnDirectorySynchronised(Pusher.DirectoryPushedEventArgs e)
        {
            WriteInformation(String.Format("Directory synchronised. Path: \"{0}\".", e.Path));
            // TODO:: output summary numbers as well
        }

        public override void OnFileCreated(EntryEventArgs e)
        {
            WriteWarning(String.Format("File created. Path: \"{0}\".", e.Path));           
        }

        public override void OnFileUpdated(EntryEventArgs e)
        {
            WriteWarning(String.Format("File updated. Path: \"{0}\".", e.Path));
        }

        public override void OnFileVerified(EntryEventArgs e)
        {
            WriteInformation(String.Format("File verified. Path: \"{0}\".", e.Path));
        }
    
        public void WriteError(string message) 
        {
            Write(ConsoleColor.Red, message);
        }

        public void WriteWarning(string message)
        {
            Write(ConsoleColor.Yellow, message);
        }

        public void WriteInformation(string message)
        {
            Write(ConsoleColor.White, message);
        }

        private void Write(ConsoleColor color, string message)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);        
        }

    }

}
