using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowLibrary
{
    public enum SynchronisationAction
    {
        Copy,
        Delete
    }

    public enum Direction
    {
        Push,
        Pull
    }

    public enum SyncResult
    {
        Ignored,
        Unchanged,
        Created,
        Deleted,
        Updated,
        Failed
    }

    public enum SubscriptionLevel
    {
        Synchroniser,// add none? TODO:: retsyle as Root/Backup/Job?
        Directory,
        File
    }

    //public const string MatchAllPattern = "*.*";
}
