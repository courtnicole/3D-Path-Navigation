namespace PathNav.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;

    public class PathSortedList<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
    {
        #region Local Variables
        private TKey[] _keys;
        private TValue[] _values;
        private int _version;
        private KeyList _keyList;
        private ValueList _valueList;

        [NonSerialized] private object _syncRoot;

        private static TKey[] _emptyKeys = new TKey[0];
        private static TValue[] _emptyValues = new TValue[0];

        private const int _defaultCapacity = 4;

        private const int _maxArrayLength = 0X7FEFFFFF;
        #endregion

        #region Properties
        public int Capacity
        {
            get => _keys.Length;
            set
            {
                if (value == _keys.Length) return;

                if (value < Count) throw new ArgumentOutOfRangeException();

                if (value > 0)
                {
                    TKey[]   newKeys   = new TKey[value];
                    TValue[] newValues = new TValue[value];

                    if (Count > 0)
                    {
                        Array.Copy(_keys,   0, newKeys,   0, Count);
                        Array.Copy(_values, 0, newValues, 0, Count);
                    }

                    _keys   = newKeys;
                    _values = newValues;
                }
                else
                {
                    _keys   = _emptyKeys;
                    _values = _emptyValues;
                }
            }
        }

        public IComparer<TKey> Comparer { get; }

        public int Count { get; private set; }

        public bool ContainsKey(TKey key) => IndexOfKey(key) >= 0;

        public bool ContainsValue(TValue value) => IndexOfValue(value) >= 0;

        #region Key Properties
        public IList<TKey> Keys => GetKeyListHelper();

        public TValue this[TKey key]
        {
            get
            {
                int i = IndexOfKey(key);

                if (i >= 0)
                    return _values[i];

                throw new KeyNotFoundException();
            }
            set
            {
                if (key == null)
                    throw new ArgumentNullException(nameof(key));

                int i = Array.BinarySearch(_keys, 0, Count, key, Comparer);

                if (i >= 0)
                {
                    _values[i] = value;
                    _version++;
                    return;
                }

                Insert(~i, key, value);
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => GetKeyListHelper();

        ICollection IDictionary.Keys => GetKeyListHelper();

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => GetKeyListHelper();

        object IDictionary.this[object key]
        {
            get
            {
                if (!IsCompatibleKey(key)) return null;

                int i = IndexOfKey((TKey)key);

                if (i >= 0) return _values[i];

                return null;
            }
            set
            {
                if (!IsCompatibleKey(key)) throw new ArgumentNullException();

                if (value == null && default(TKey) != null)
                    throw new ArgumentNullException(nameof(value), @"Key is null.");

                try
                {
                    TKey tempKey = (TKey)key;

                    try
                    {
                        this[tempKey] = (TValue)value;
                    }
                    catch (InvalidCastException)
                    {
                        throw new InvalidCastException($"Value type for {value} of {typeof(TValue)} is not supported.");
                    }
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException($"Key type for {value} of {typeof(TKey)} is not supported.");
                }
            }
        }
        #endregion

        #region Value Properties
        public IList<TValue> Values => GetValueListHelper();

        ICollection<TValue> IDictionary<TKey, TValue>.Values => GetValueListHelper();

        ICollection IDictionary.Values => GetValueListHelper();

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => GetValueListHelper();
        #endregion

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;

        bool IDictionary.IsReadOnly => false;

        bool IDictionary.IsFixedSize => false;

        bool ICollection.IsSynchronized => false;

        bool IDictionary.Contains(object key)
        {
            if (IsCompatibleKey(key)) return ContainsKey((TKey)key);

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);

            return index >= 0 && EqualityComparer<TValue>.Default.Equals(_values[index], keyValuePair.Value);
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null) Interlocked.CompareExchange(ref _syncRoot, new object(), null);

                return _syncRoot;
            }
        }
        #endregion

        #region Constructors
        public PathSortedList()
        {
            _keys = _emptyKeys;
            _values = _emptyValues;
            Count = 0;
            Comparer = Comparer<TKey>.Default;
        }

        public PathSortedList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException();

            _keys = new TKey[capacity];
            _values = new TValue[capacity];
            Comparer = Comparer<TKey>.Default;
        }

        public PathSortedList(IComparer<TKey> comparer)
            : this()
        {
            if (comparer != null) Comparer = comparer;
        }

        public PathSortedList(int capacity, IComparer<TKey> comparer) : this(comparer) => Capacity = capacity;

        public PathSortedList(IDictionary<TKey, TValue> dictionary) : this(dictionary, null) { }

        public PathSortedList(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
            : this(dictionary?.Count ?? 0, comparer)
        {
            if (dictionary == null)
                throw new ArgumentNullException();

            dictionary.Keys.CopyTo(_keys, 0);
            dictionary.Values.CopyTo(_values, 0);
            Array.Sort(_keys, _values, comparer);
            Count = dictionary.Count;
        } 
        #endregion

        #region Get Key
        public int IndexOfKey(TKey key)
        {
            if (key == null)
                throw new ArgumentNullException();

            int ret = Array.BinarySearch(_keys, 0, Count, key, Comparer);
            return ret >= 0 ? ret : -1;
        }

        private TKey GetKey(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            return _keys[index];
        }
        #endregion

        #region Get Value
        public int IndexOfValue(TValue value) => Array.IndexOf(_values, value, 0, Count);

        private TValue GetByIndex(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            return _values[index];
        }
        #endregion

        #region Get Enumerator
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => new Enumerator(this, Enumerator.keyValuePair);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => new Enumerator(this, Enumerator.keyValuePair);

        IDictionaryEnumerator IDictionary.GetEnumerator() => new Enumerator(this, Enumerator.dictEntry);

        IEnumerator IEnumerable.GetEnumerator() => new Enumerator(this, Enumerator.keyValuePair);
        #endregion

        #region Try Get Value
        public bool TryGetValue(TKey key, out TValue value)
        {
            int i = IndexOfKey(key);

            if (i >= 0)
            {
                value = _values[i];
                return true;
            }

            value = default;
            return false;
        }
        #endregion

        #region Add
        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException();

            int i = Array.BinarySearch(_keys, 0, Count, key, Comparer);

            if (i >= 0)
            {
                ArgumentException exception = new(@"Item with the same key has already been added", key.ToString());
                throw exception;
            }

            Insert(~i, key, value);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
        {
            Add(keyValuePair.Key, keyValuePair.Value);
        }

        void IDictionary.Add(object key, object value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key), @"Key is null");

            if (value == null && default(TValue) != null)
                throw new ArgumentNullException(nameof(value), @"Value is null.");

            try
            {
                TKey tempKey = (TKey)key;

                try
                {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException)
                {
                    throw new InvalidCastException($"Value type for {value} of {typeof(TValue)} is not supported.");
                }
            }
            catch (InvalidCastException)
            {
                throw new InvalidCastException($"Key type for {value} of {typeof(TKey)} is not supported.");
            }
        }
        #endregion

        #region Insert
        private void Insert(int index, TKey key, TValue value)
        {
            if (Count == _keys.Length)
                EnsureCapacity(Count + 1);

            if (index < Count)
            {
                Array.Copy(_keys,   index, _keys,   index + 1, Count - index);
                Array.Copy(_values, index, _values, index + 1, Count - index);
            }

            _keys[index]   = key;
            _values[index] = value;
            Count++;
            _version++;
        }
        #endregion

        #region Copy To
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count) throw new IndexOutOfRangeException(nameof(arrayIndex));

            for (int i = 0; i < Count; i++)
            {
                KeyValuePair<TKey, TValue> entry = new(_keys[i], _values[i]);
                array[arrayIndex + i] = entry;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));

            if (array.Rank != 1) throw new ArgumentException($"Array rank {array.Rank} > 1 is not supported.");

            if (array.GetLowerBound(0) != 0) throw new ArgumentException($"Non-zero lower bound {array.GetLowerBound(0)} is invalid.");

            if (arrayIndex < 0 || arrayIndex > array.Length) throw new IndexOutOfRangeException(nameof(arrayIndex));

            if (array.Length - arrayIndex < Count) throw new IndexOutOfRangeException(nameof(arrayIndex));

            KeyValuePair<TKey, TValue>[] keyValuePairArray = array as KeyValuePair<TKey, TValue>[];

            if (keyValuePairArray != null)
            {
                for (int i = 0; i < Count; i++)
                {
                    keyValuePairArray[i + arrayIndex] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                }
            }
            else
            {
                object[] objects = array as object[];

                if (objects == null) throw new ArgumentNullException(nameof(array));

                try
                {
                    for (int i = 0; i < Count; i++)
                    {
                        objects[i + arrayIndex] = new KeyValuePair<TKey, TValue>(_keys[i], _values[i]);
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArrayTypeMismatchException();
                }
            }
        }
        #endregion

        #region Remove
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            Count--;

            if (index < Count)
            {
                Array.Copy(_keys,   index + 1, _keys,   index, Count - index);
                Array.Copy(_values, index + 1, _values, index, Count - index);
            }

            _keys[Count]   = default;
            _values[Count] = default;
            _version++;
        }

        public bool Remove(TKey key)
        {
            int i = IndexOfKey(key);

            if (i >= 0)
                RemoveAt(i);
            return i >= 0;
        }

        void IDictionary.Remove(object key)
        {
            if (IsCompatibleKey(key)) Remove((TKey)key);
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
        {
            int index = IndexOfKey(keyValuePair.Key);

            if (index >= 0 && EqualityComparer<TValue>.Default.Equals(_values[index], keyValuePair.Value))
            {
                RemoveAt(index);
                return true;
            }

            return false;
        }
        #endregion

        #region Clear
        public void Clear()
        {
            _version++;
            Array.Clear(_keys,   0, Count);
            Array.Clear(_values, 0, Count);
            Count = 0;
        }

        public void TrimExcess()
        {
            int threshold = (int)(_keys.Length * 0.9);

            if (Count < threshold) Capacity = Count;
        }
        #endregion

        #region Helpers
        private KeyList GetKeyListHelper()
        {
            return _keyList ??= new KeyList(this);
        }

        private ValueList GetValueListHelper()
        {
            return _valueList ??= new ValueList(this);
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = _keys.Length == 0 ? _defaultCapacity : _keys.Length * 2;
            if ((uint)newCapacity > _maxArrayLength) newCapacity = _maxArrayLength;
            if (newCapacity       < min) newCapacity             = min;
            Capacity = newCapacity;
        }

        private static bool IsCompatibleKey(object key)
        {
            if (key == null) throw new ArgumentNullException();

            return key is TKey;
        }
        #endregion

        #region Private Structs and Classes
        [Serializable]
        private struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            private PathSortedList<TKey, TValue> _sortedList;
            private TKey _key;
            private TValue _value;
            private int _index;
            private int _version;
            private int _getEnumeratorRetType;

            internal const int keyValuePair = 1;
            internal const int dictEntry = 2;

            internal Enumerator(PathSortedList<TKey, TValue> sortedList, int getEnumeratorRetType)
            {
                _sortedList           = sortedList;
                _index                = 0;
                _version              = _sortedList._version;
                _getEnumeratorRetType = getEnumeratorRetType;
                _key                  = default;
                _value                = default;
            }

            public void Dispose()
            {
                _index = 0;
                _key   = default;
                _value = default;
            }

            object IDictionaryEnumerator.Key
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    return _key;
                }
            }

            public bool MoveNext()
            {
                if (_version != _sortedList._version)
                    throw new InvalidOperationException();

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    _key   = _sortedList._keys[_index];
                    _value = _sortedList._values[_index];
                    _index++;
                    return true;
                }

                _index = _sortedList.Count + 1;
                _key   = default;
                _value = default;
                return false;
            }

            DictionaryEntry IDictionaryEnumerator.Entry
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    return new DictionaryEntry(_key, _value);
                }
            }

            public KeyValuePair<TKey, TValue> Current => new(_key, _value);

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    if (_getEnumeratorRetType == dictEntry) return new DictionaryEntry(_key, _value);

                    return new KeyValuePair<TKey, TValue>(_key, _value);
                }
            }

            object IDictionaryEnumerator.Value
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    return _value;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList._version) throw new InvalidOperationException();

                _index = 0;
                _key   = default;
                _value = default;
            }
        }

        [Serializable]
        private sealed class SortedListKeyEnumerator : IEnumerator<TKey>
        {
            private PathSortedList<TKey, TValue> _sortedList;
            private int _index;
            private int _version;

            internal SortedListKeyEnumerator(PathSortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version    = sortedList._version;
            }

            public void Dispose()
            {
                _index  = 0;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList._version) throw new InvalidOperationException();

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    Current = _sortedList._keys[_index];
                    _index++;
                    return true;
                }

                _index  = _sortedList.Count + 1;
                Current = default;
                return false;
            }

            public TKey Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList._version) throw new InvalidOperationException();

                _index  = 0;
                Current = default;
            }
        }

        [Serializable]
        private sealed class SortedListValueEnumerator : IEnumerator<TValue>
        {
            private PathSortedList<TKey, TValue> _sortedList;
            private int _index;
            private int _version;

            internal SortedListValueEnumerator(PathSortedList<TKey, TValue> sortedList)
            {
                _sortedList = sortedList;
                _version    = sortedList._version;
            }

            public void Dispose()
            {
                _index  = 0;
                Current = default;
            }

            public bool MoveNext()
            {
                if (_version != _sortedList._version) throw new InvalidOperationException();

                if ((uint)_index < (uint)_sortedList.Count)
                {
                    Current = _sortedList._values[_index];
                    _index++;
                    return true;
                }

                _index  = _sortedList.Count + 1;
                Current = default;
                return false;
            }

            public TValue Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _sortedList.Count + 1) throw new InvalidOperationException();

                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _sortedList._version) throw new InvalidOperationException();

                _index  = 0;
                Current = default;
            }
        }

        [Serializable]
        private sealed class KeyList : IList<TKey>, ICollection
        {
            private PathSortedList<TKey, TValue> _dict;

            internal KeyList(PathSortedList<TKey, TValue> dictionary) => _dict = dictionary;

            public int Count => _dict.Count;

            public bool IsReadOnly => true;

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

            public void Add(TKey key)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TKey key) => _dict.ContainsKey(key!);

            public void CopyTo(TKey[] array, int arrayIndex)
            {
                Array.Copy(_dict._keys, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if (array != null && array.Rank != 1)
                    throw new ArgumentException();

                try
                {
                    Array.Copy(_dict._keys, 0, array, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArgumentException();
                }
            }

            public void Insert(int index, TKey value)
            {
                throw new NotSupportedException();
            }

            public TKey this[int index]
            {
                get => _dict.GetKey(index);
                set => throw new NotSupportedException();
            }

            public IEnumerator<TKey> GetEnumerator() => new SortedListKeyEnumerator(_dict);

            IEnumerator IEnumerable.GetEnumerator() => new SortedListKeyEnumerator(_dict);

            public int IndexOf(TKey key)
            {
                if (key == null)
                    throw new ArgumentNullException();

                int i = Array.BinarySearch(_dict._keys, 0,
                                           _dict.Count, key, _dict.Comparer);
                if (i >= 0) return i;

                return -1;
            }

            public bool Remove(TKey key) => throw new NotSupportedException();

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }

        [Serializable]
        private sealed class ValueList : IList<TValue>, ICollection
        {
            private PathSortedList<TKey, TValue> _dict;

            internal ValueList(PathSortedList<TKey, TValue> dictionary) => _dict = dictionary;

            public int Count => _dict.Count;

            public bool IsReadOnly => true;

            bool ICollection.IsSynchronized => false;

            object ICollection.SyncRoot => ((ICollection)_dict).SyncRoot;

            public void Add(TValue key)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(TValue value) => _dict.ContainsValue(value);

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                Array.Copy(_dict._values, 0, array, arrayIndex, _dict.Count);
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                if (array != null && array.Rank != 1)
                    throw new ArgumentException();

                try
                {
                    Array.Copy(_dict._values, 0, array, arrayIndex, _dict.Count);
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArrayTypeMismatchException();
                }
            }

            public void Insert(int index, TValue value)
            {
                throw new NotSupportedException();
            }

            public TValue this[int index]
            {
                get => _dict.GetByIndex(index);
                set => throw new NotSupportedException();
            }

            public IEnumerator<TValue> GetEnumerator() => new SortedListValueEnumerator(_dict);

            IEnumerator IEnumerable.GetEnumerator() => new SortedListValueEnumerator(_dict);

            public int IndexOf(TValue value) => Array.IndexOf(_dict._values, value, 0, _dict.Count);

            public bool Remove(TValue value) => throw new NotSupportedException();

            public void RemoveAt(int index)
            {
                throw new NotSupportedException();
            }
        }
        #endregion
    }
}