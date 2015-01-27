// 
//  Copyright 2011 Ekon Benefits
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;

namespace RazorEngine.Compilation.ImpromptuInterface.Optimization
{
    internal class BareBonesList<T> : ICollection<T>
    {
        private T[] _list;
        private int _addIndex;

        private int _length;


        /// <summary>
        /// Initializes a new instance of the <see cref="BareBonesList&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="length">The max length that the list cannot grow beyound</param>
        public BareBonesList(int length)
        {
            _list = new T[length];
            _length = length;
        }

        public void Add(T item)
        {
            _list[_addIndex++] = item;
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(_list, arrayIndex, array, 0, _length);
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count
        {
            get { return _length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the enumerator. with bare bones this is good only once
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new BareBonesEnumerator(_list, _addIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        internal class BareBonesEnumerator : IEnumerator<T>
        {
            private T[] _list;
            private int _enumerateInex = -1;
            private int _length;

            public BareBonesEnumerator(T[] list, int length)
            {
                _list = list;
                _length = length;
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                _enumerateInex++;
                return _enumerateInex < _length;
            }

            public void Reset()
            {
                _enumerateInex = 0;
            }

            public T Current
            {
                get { return _list[_enumerateInex]; }
            }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }

    }


}