using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronisationLibrary
{
    public abstract class CleanerSubscriber : IDisposable
    {
        private bool _disposed = false;

        public CleanerSubscriber()
        {
            Cleaner.FileMatched += new Cleaner.EntryEventHandler(OnFileMatched);
            Cleaner.EntryDeleted += new Cleaner.EntryEventHandler(OnEntryDeleted);
            Cleaner.DirectoryCleaned += new Cleaner.DirectoryCleanedEventHandler(OnDirectoryCleaned);
            Cleaner.Error += new Cleaner.EntryErrorEventHandler(OnError);       
        }
        
        public virtual void OnFileMatched(EntryEventArgs e)
        { }

        public virtual void OnEntryDeleted(EntryEventArgs e)
        { }

        public virtual void OnDirectoryCleaned(Cleaner.DirectoryCleanedEventArgs e)
        { }

        public virtual void OnError(EntryErrorEventArgs e)
        { }
                
        ~CleanerSubscriber()
        {
            Dispose(false);
        }

        public void Dispose() 
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Cleaner.FileMatched -= OnFileMatched;
                    Cleaner.EntryDeleted -= OnEntryDeleted;
                    Cleaner.DirectoryCleaned -= OnDirectoryCleaned;
                    Cleaner.Error -= OnError;
                }
                _disposed = true;
            }
        }
    }
}
