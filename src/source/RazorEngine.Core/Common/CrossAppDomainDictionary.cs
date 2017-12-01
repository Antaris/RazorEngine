using System.Collections;
using System.Collections.Generic;

namespace RazorEngine.Common
{
    internal class CrossAppDomainDictionary<TKey, TValue> : CrossAppDomainObject, IDictionary<TKey, TValue>
    {
        #region Fields

        private readonly IDictionary<TKey, TValue> _dict;

        #endregion

        #region Constructor

        public CrossAppDomainDictionary()
        {
            _dict = new Dictionary<TKey, TValue>();
        }

        public CrossAppDomainDictionary(int capacity)
        {
            _dict = new Dictionary<TKey, TValue>(capacity);
        }

        public CrossAppDomainDictionary(IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(comparer);
        }

        public CrossAppDomainDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dict = new Dictionary<TKey, TValue>(dictionary);
        }

        public CrossAppDomainDictionary(int capacity, IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        public CrossAppDomainDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _dict = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        #endregion

        #region Properties

        public TValue this[TKey key] { get { return _dict[key]; } set { _dict[key] = value; } }

        public ICollection<TKey> Keys { get { return _dict.Keys; } }

        public ICollection<TValue> Values { get { return _dict.Values; } }

        public int Count { get { return _dict.Count; } }

        public bool IsReadOnly { get { return _dict.IsReadOnly; } }

        #endregion

        #region Methods

        public void Add(TKey key, TValue value)
        {
            _dict.Add(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dict.Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public bool Remove(TKey key)
        {
            return _dict.Remove(key);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _dict.Remove(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        #endregion
    }
}
