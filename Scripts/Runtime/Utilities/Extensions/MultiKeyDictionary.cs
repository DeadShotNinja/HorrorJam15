using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace HJ.Tools
{
    public class MultiKeyDictionary<K, L, V>
    {
        internal readonly Dictionary<K, V> BaseDictionary = new ();
        internal readonly Dictionary<L, K> SubDictionary = new ();
        internal readonly Dictionary<K, L> PrimaryToSubkeyMapping = new ();
        internal readonly ReaderWriterLockSlim ReaderWriterLock = new ();

        public V this[L subKey]
        {
            get
            {
                if (TryGetValue(subKey, out V item))
                    return item;

                throw new KeyNotFoundException("sub key not found: " + subKey.ToString());
            }
        }

        public V this[K primaryKey]
        {
            get
            {
                if (TryGetValue(primaryKey, out V item))
                    return item;

                throw new KeyNotFoundException("primary key not found: " + primaryKey.ToString());
            }
        }

        public void Associate(L subKey, K primaryKey)
        {
            ReaderWriterLock.EnterUpgradeableReadLock();

            try
            {
                if (!BaseDictionary.ContainsKey(primaryKey))
                    throw new KeyNotFoundException(string.Format("The base dictionary does not contain the key '{0}'", primaryKey));

                if (PrimaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    ReaderWriterLock.EnterWriteLock();

                    try
                    {
                        if (SubDictionary.ContainsKey(PrimaryToSubkeyMapping[primaryKey]))
                        {
                            SubDictionary.Remove(PrimaryToSubkeyMapping[primaryKey]);
                        }

                        PrimaryToSubkeyMapping.Remove(primaryKey);
                    }
                    finally
                    {
                        ReaderWriterLock.ExitWriteLock();
                    }
                }

                SubDictionary[subKey] = primaryKey;
                PrimaryToSubkeyMapping[primaryKey] = subKey;
            }
            finally
            {
                ReaderWriterLock.ExitUpgradeableReadLock();
            }
        }

        public bool TryGetValue(L subKey, out V val)
        {
            val = default;
            ReaderWriterLock.EnterReadLock();

            try
            {
                if (SubDictionary.TryGetValue(subKey, out K primaryKey))
                {
                    return BaseDictionary.TryGetValue(primaryKey, out val);
                }
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }

            return false;
        }

        public bool TryGetValue(K primaryKey, out V val)
        {
            ReaderWriterLock.EnterReadLock();

            try
            {
                return BaseDictionary.TryGetValue(primaryKey, out val);
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }
        }

        public bool ContainsKey(L subKey)
        {
            return TryGetValue(subKey, out _);
        }

        public bool ContainsKey(K primaryKey)
        {
            return TryGetValue(primaryKey, out _);
        }

        public void Remove(K primaryKey)
        {
            ReaderWriterLock.EnterWriteLock();

            try
            {
                if (PrimaryToSubkeyMapping.ContainsKey(primaryKey))
                {
                    if (SubDictionary.ContainsKey(PrimaryToSubkeyMapping[primaryKey]))
                    {
                        SubDictionary.Remove(PrimaryToSubkeyMapping[primaryKey]);
                    }

                    PrimaryToSubkeyMapping.Remove(primaryKey);
                }

                BaseDictionary.Remove(primaryKey);
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }

        public void Remove(L subKey)
        {
            ReaderWriterLock.EnterWriteLock();

            try
            {
                BaseDictionary.Remove(SubDictionary[subKey]);

                PrimaryToSubkeyMapping.Remove(SubDictionary[subKey]);

                SubDictionary.Remove(subKey);
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }

        public void Add(K primaryKey, V val)
        {
            ReaderWriterLock.EnterWriteLock();

            try
            {
                BaseDictionary.Add(primaryKey, val);
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }

        public void Add(K primaryKey, L subKey, V val)
        {
            Add(primaryKey, val);
            Associate(subKey, primaryKey);
        }

        public V[] CloneValues()
        {
            ReaderWriterLock.EnterReadLock();

            try
            {
                V[] values = new V[BaseDictionary.Values.Count];
                BaseDictionary.Values.CopyTo(values, 0);
                return values;
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }
        }

        public List<V> Values
        {
            get
            {
                ReaderWriterLock.EnterReadLock();

                try
                {
                    return BaseDictionary.Values.ToList();
                }
                finally
                {
                    ReaderWriterLock.ExitReadLock();
                }
            }
        }

        public K[] ClonePrimaryKeys()
        {
            ReaderWriterLock.EnterReadLock();

            try
            {
                K[] values = new K[BaseDictionary.Keys.Count];
                BaseDictionary.Keys.CopyTo(values, 0);
                return values;
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }
        }

        public L[] CloneSubKeys()
        {
            ReaderWriterLock.EnterReadLock();

            try
            {
                L[] values = new L[SubDictionary.Keys.Count];
                SubDictionary.Keys.CopyTo(values, 0);
                return values;
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }
        }

        public void Clear()
        {
            ReaderWriterLock.EnterWriteLock();

            try
            {
                BaseDictionary.Clear();
                SubDictionary.Clear();
                PrimaryToSubkeyMapping.Clear();
            }
            finally
            {
                ReaderWriterLock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                ReaderWriterLock.EnterReadLock();

                try
                {
                    return BaseDictionary.Count;
                }
                finally
                {
                    ReaderWriterLock.ExitReadLock();
                }
            }
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            ReaderWriterLock.EnterReadLock();

            try
            {
                return BaseDictionary.GetEnumerator();
            }
            finally
            {
                ReaderWriterLock.ExitReadLock();
            }
        }
    }
}
