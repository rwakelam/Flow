using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{

    public abstract class Subscriber : IDisposable
    {

        private List<Synchroniser> _Publishers;
        private bool _Disposed = false;
        private SubscriptionLevel _SubscriptionLevel;

        public SubscriptionLevel SubscriptionLevel 
        {
            get 
            {
                return _SubscriptionLevel;
            }
            private set
            {
                _SubscriptionLevel = value;
            }
        }

        public Subscriber(SubscriptionLevel subscriptionLevel = SubscriptionLevel.Synchroniser)
        {
            _Publishers = new List<Synchroniser>();
            SubscriptionLevel = subscriptionLevel;
        }

        public void Subscribe(Synchroniser publisher)
        {
            if (publisher == null)
            {
                throw new ArgumentNullException("Publisher");
            }
            _Publishers.Add(publisher);
            publisher.SyncStarted += new Synchroniser.SyncEventHandler(OnSyncStarted);
            publisher.SyncCompleted += new Synchroniser.SyncEventHandler(OnSyncCompleted);
            publisher.Error += new Synchroniser.SyncErrorEventHandler(OnError);
            if (SubscriptionLevel == SubscriptionLevel.Synchroniser)
            {
                return;
            }
            publisher.DirectorySyncCompleted += new Synchroniser.DirectorySyncEventHandler(OnDirectorySyncCompleted);
            if (SubscriptionLevel == SubscriptionLevel.Directory) 
            {
                return;
            }
            publisher.FileSyncCompleted += new Synchroniser.FileSyncEventHandler(OnFileSyncCompleted);
        }
        
        public void Unsubscribe(Synchroniser publisher)
        {
            _Publishers.Remove(publisher);
            publisher.SyncStarted -= OnSyncStarted;
            publisher.SyncCompleted -= OnSyncCompleted;
            publisher.Error -= OnError;
            if (SubscriptionLevel == SubscriptionLevel.Synchroniser)
            {
                return;
            }
            publisher.DirectorySyncCompleted -= OnDirectorySyncCompleted;
            if (SubscriptionLevel == SubscriptionLevel.Directory)
            {
                return;
            }
            publisher.FileSyncCompleted -= OnFileSyncCompleted;
        }

        ~Subscriber()
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
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Publishers.ForEach(x => Unsubscribe(x));
                }
                _Disposed = true;
            }
        }

        public virtual void OnSyncStarted(object sender, SyncEventArgs e)
        {}

        public virtual void OnSyncCompleted(object sender, SyncEventArgs e)
        {}

        public virtual void OnFileSyncCompleted(object sender, FileSyncEventArgs e) 
        {}

        public virtual void OnDirectorySyncCompleted(object sender, DirectorySyncEventArgs e) 
        {}

        public virtual void OnError(object sender, SyncErrorEventArgs e) 
        {}
        
    }

}
