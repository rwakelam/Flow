using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{
    public abstract class PusherSubscriber : IDisposable
    {        
        private bool _disposed = false;

        public PusherSubscriber()
        {
            Pusher.DirectoryCreated += new Pusher.EntryEventHandler(OnDirectoryCreated);
            Pusher.DirectoryUpdated += new Pusher.EntryEventHandler(OnDirectoryUpdated);
            Pusher.FileVerified += new Pusher.EntryEventHandler(OnFileVerified);
            Pusher.FileCreated += new Pusher.EntryEventHandler(OnFileCreated);
            Pusher.FileUpdated += new Pusher.EntryEventHandler(OnFileUpdated);
            Pusher.DirectoryPushed += new Pusher.DirectoryPushedEventHandler(OnDirectorySynchronised);
            Pusher.Error += new Pusher.EntryErrorEventHandler(OnError);  
     
        }
        
        public virtual void OnDirectoryCreated(EntryEventArgs e)
        { }

        public virtual void OnDirectoryUpdated(EntryEventArgs e)
        { }

        public virtual void OnDirectorySynchronised(Pusher.DirectoryPushedEventArgs e)
        { }

        public virtual void OnFileVerified(EntryEventArgs e)
        { }

        public virtual void OnFileUpdated(EntryEventArgs e)
        { }

        public virtual void OnFileCreated(EntryEventArgs e)
        { }

        public virtual void OnError(EntryErrorEventArgs e)
        { }
                
        ~PusherSubscriber()
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
                    Pusher.DirectoryCreated -= OnDirectoryCreated;
                    Pusher.DirectoryUpdated -= OnDirectoryUpdated;
                    Pusher.DirectoryPushed -= OnDirectorySynchronised;
                    Pusher.FileVerified -= OnFileVerified;
                    Pusher.FileCreated -= OnFileCreated;
                    Pusher.FileUpdated -= OnFileUpdated;
                    Cleaner.Error -= OnError;
                }
                _disposed = true;
            }
        }

    }
}
