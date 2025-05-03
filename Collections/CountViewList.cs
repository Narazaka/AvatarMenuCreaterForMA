using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System;

namespace net.narazaka.avatarmenucreator.collections
{
    public class CountViewList : IList<int>, IList
    {
        public CountViewList(Func<int> getCount, Action<int> setCount)
        {
            _getCount = getCount;
            _setCount = setCount;
        }

        Func<int> _getCount;
        Action<int> _setCount;
        int _count
        {
            get => _getCount();
            set => _setCount(value);
        }
        public int Count => _count;
        public bool IsReadOnly => false;
        bool IList.IsFixedSize => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => null;
        public int this[int index]
        {
            get => index;
            set { }
        }
        object IList.this[int index]
        {
            get => index;
            set { }
        }
        public void Add(int item) => _count++;
        int IList.Add(object item) => _count++;
        public void Clear() => _count = 0;
        public bool Contains(int item) => 0 <= item && item < _count;
        bool IList.Contains(object item) => item is int && Contains((int)item);
        public void CopyTo(int[] array, int arrayIndex) => Enumerable.Range(0, _count).ToArray().CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => Enumerable.Range(0, _count).ToArray().CopyTo(array, index);
        public IEnumerator<int> GetEnumerator() => Enumerable.Range(0, _count).GetEnumerator();
        public int IndexOf(int item) => Contains(item) ? item : -1;
        int IList.IndexOf(object item) => item is int ? IndexOf((int)item) : -1;
        public void Insert(int index, int item) => _count++;
        void IList.Insert(int index, object item) => _count++;
        public bool Remove(int item)
        {
            if (!Contains(item)) return false;
            _count--;
            return true;
        }
        void IList.Remove(object item)
        {
            if (item is int) Remove((int)item);
        }
        public void RemoveAt(int index) => Remove(index);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
