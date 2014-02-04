using System.Collections.Generic;

namespace TPC.Processor
{
    public interface IProcessor<TKey, TResult>
    {
        void Start();
        void Stop();
        void Add(IEnumerable<TKey> args);
        int KeyCount { get; }
        int ProcessedKeyCount { get; }
        List<TKey> Get(int count);
        event ProcessCompleteDelegate<TKey, TResult> OnProcessComplete;
    }
}
