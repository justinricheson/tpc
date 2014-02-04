using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using TPC.Utilities;

namespace TPC.Processor
{
    public class MD5HashTableProcessor : IProcessor<string, string>
    {
        #region Vars
        private HashSet<string> _keys;
        private Dictionary<string, string> _results;
        private Thread _t;
        private bool _process;
        public int KeyCount { get { return _keys.Count; } }
        public int ProcessedKeyCount { get { return _results.Count; } }
        #endregion

        #region Public
        public event ProcessCompleteDelegate<string, string> OnProcessComplete;
        public MD5HashTableProcessor()
        {
            _keys = new HashSet<string>();
            _results = new Dictionary<string, string>();
        }
        public void Start()
        {
            _process = true;
            _t = new Thread(Process);
            
            _t.Start();
        }
        public void Stop()
        {
            _process = false;
        }
        public void Add(IEnumerable<string> args)
        {
            lock (_keys)
            {
                foreach (var key in args)
                    _keys.Add(key);
            }
        }
        public List<string> Get(int count)
        {
            lock (_keys)
            {
                var clone = new List<string>();

                while (count > 0)
                {
                    if (_keys.Count > 0)
                    {
                        string key = _keys.First();
                        clone.Add(key);
                        _keys.Remove(key);
                    }
                    else break;

                    count--;
                }

                return clone;
            }
        }
        public static List<string> GenerateKeys()
        {
            LogBroker.Log("Generating input keys");

            var keys = new List<string>();
            using (var rdr = new StreamReader(GetDictionaryPath()))
            {
                while (rdr.Peek() > -1)
                    keys.Add(rdr.ReadLine());
            }
            return keys;
        }
        #endregion

        #region Private
        private void Process()
        {
            while (_process)
            {
                lock (_keys)
                {
                    if (_keys.Count > 0)
                    {
                        string key = _keys.First();
                        string hash = Hash(key);
                        _results.Add(key, hash);
                        _keys.Remove(key);

                        LogBroker.Log(string.Format("{0} - {1}", key, hash)); 
                    }
                    else
                    {
                        NotifyProcessComplete();
                        break;
                    }
                }
            }
        }
        private string Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
                return md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input)).ToHexString();
        }
        private void NotifyProcessComplete()
        {
            if (OnProcessComplete != null)
            {
                var clone = new Dictionary<string, string>();
                foreach (var key in _results.Keys)
                    clone.Add(key, _results[key]);

                var args = new ProcessCompleteEventArgs<string, string>();
                args.Results = clone;

                OnProcessComplete(this, args);
            }
        }
        private static string GetDictionaryPath()
        {
            string fullPath = Assembly.GetExecutingAssembly().Location;
            string directory = Path.GetDirectoryName(fullPath) + Constants.RESOURCES_SUBDIR;

            return Path.Combine(directory, Constants.DICTIONARY_FILENAME);
        }
        #endregion
    }
}
