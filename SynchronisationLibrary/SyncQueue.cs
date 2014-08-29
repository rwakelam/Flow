using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SynchronisationLibrary
{
    // treat it like wix; describe the structure that you want to end up with?
    // how would deletions work?
    // use backup to handle deletions?
    // separate clean out as separate function?
    // delete file from tree if its there; if the sync has started copy could fail anyway
    // but detritus will begin to pile up on server side
    // tree implementer will need to be a bit smarter but that's ok

    public class SyncQueue : IEnumerable<KeyValuePair<string, SynchronisationAction>> 
    {
        //TODO:: implement load/save queue? xml serialise/deserialise
        private Dictionary<string, SynchronisationAction> _actions;
        private static ManualResetEvent _enableEvent = new ManualResetEvent(true);
        private object _lock = new object();

        public SyncQueue() 
        {
            _actions = new Dictionary<string, SynchronisationAction>();
        }

        public void Enqueue(string relativePath, SynchronisationAction action)
        {
            _enableEvent.WaitOne();
            lock (_lock)
            {
                if (_actions.ContainsKey(relativePath))
                {
                    SynchronisationAction oldAction = _actions[relativePath];
                    if (oldAction == action)
                    {
                        return;
                    }
                    if (oldAction == SynchronisationAction.Copy)
                    {
                        if (action == SynchronisationAction.Delete)
                        {
                            _actions.Remove(relativePath);
                        }
                        return;
                    }
                    _actions.Remove(relativePath);
                }
                _actions.Add(relativePath, action);
            }
        }

        public Dictionary<string, SynchronisationAction> Dequeue()
        {
            _enableEvent.Reset();
            Dictionary<string, SynchronisationAction> result =
                new Dictionary<string, SynchronisationAction>(_actions);
            _actions.Clear();
            _enableEvent.Set();
            return result;        
        }

        public SynchronisationAction Dequeue(string relativePath)
        {
            SynchronisationAction result;
            _enableEvent.Reset();
            result = _actions[relativePath];
            _actions.Remove(relativePath);
            _enableEvent.Set();
            return result;
        }

        public IEnumerator<KeyValuePair<string, SynchronisationAction>> GetEnumerator()
        {
            foreach (KeyValuePair<string, SynchronisationAction> kvp in _actions)
            {
                yield return kvp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
