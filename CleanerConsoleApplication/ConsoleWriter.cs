using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SynchronisationLibrary;

namespace CleanerConsoleApplication
{

    internal class ConsoleWriter : CleanerSubscriber
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
        
        public override void OnError(EntryErrorEventArgs e)
        {
            WriteError(String.Format("Entry error. Path: \"{0}\". Message: \"{1}\".", e.Path, e.Exception.Message));
        }
        
        public override void OnEntryDeleted(EntryEventArgs e)
        {
            WriteWarning(String.Format("Entry deleted. Path: \"{0}\".", e.Path));
        }

        public override void OnFileMatched(EntryEventArgs e)
        {
            WriteInformation(String.Format("File matched. Path: \"{0}\".", e.Path));
        }
        
        public override void OnDirectoryCleaned(Cleaner.DirectoryCleanedEventArgs e)
        {
            //include subfolders in with matched?
            WriteInformation(String.Format(
                "Directory cleaned. Path: \"{0}\". Entry summary: {1} matched, {2} deleted, {3} failed.", 
                e.Path, e.Result.MatchedFiles.Count + e.Result.SubDirectoryResults.Count, 
                e.Result.DeletedEntries.Count, e.Result.FailedEntries.Count));
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
