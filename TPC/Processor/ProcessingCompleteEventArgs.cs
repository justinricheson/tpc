using System;
using System.Collections.Generic;

namespace TPC.Processor
{
    public delegate void ProcessCompleteDelegate<TKey, TResult>(object sender, ProcessCompleteEventArgs<TKey, TResult> e);
    public class ProcessCompleteEventArgs<TKey, TValue> : EventArgs
    {
        public Dictionary<TKey, TValue> Results { get; set; }
    }
}
