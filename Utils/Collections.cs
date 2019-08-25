using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PRNet.Utils.Collections {

    public class PRNetQueue<T> {

        List<T> items;

        public int Count { get { return items.Count; } }

        public PRNetQueue() {

            items = new List<T>();
        }

        public PRNetQueue(List<T> initialItems) {

            items = initialItems;
        }

        public void Push(T item) {

            if (item == null)
                return;

            items.Insert(0, item);
        }

        public T Pop() {

            T returnValue = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            return returnValue;
        }

        public List<T> PopAll() {

            List<T> returnList = items;
            items = new List<T>();

            return returnList;
        }

        public List<T> PopN(int n) {

            List<T> newList = new List<T>();

            for (int i = 0; i < n; i++) {

                if (Count > 0)
                    newList.Add(Pop());
                else
                    break;
            }

            return newList;
        }

        public T Peek() {

            if (Count == 0)
                return default(T);

            return items[items.Count - 1];
        }
    }
}
