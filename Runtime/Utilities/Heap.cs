using System;
using UnityEngine;

namespace CoreScript.Utility
{
    public class Heap<T> where T : IHeapItem<T>
    {
        T[] items;
        int currentCount;

        public int Count
        {
            get { return currentCount; }
        }

        public Heap(int maxHeapSize)
        {
            items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
            item.HeapIndex = currentCount;
            items[currentCount] = item;
            SortUp(item);
            ++currentCount;
        }

        public T RemoveFirst()
        {
            T firstItem = items[0];
            currentCount--;
            items[0] = items[currentCount];
            items[0].HeapIndex = 0;
            SortDown(items[0]);
            return firstItem;
        }

        public void UpdateItem(T item)
        {
            SortUp(item);
        }

        public bool Contains(T item)
        {
            return Equals(items[item.HeapIndex], item);
        }

        void SortDown(T item)
        {
            while (true)
            {
                int childLeftIndex = item.HeapIndex * 2 + 1;
                int childRightIndex = item.HeapIndex * 2 + 2;
                int swapIndex = 0;

                if (childLeftIndex < currentCount)
                {
                    swapIndex = childLeftIndex;

                    if (childRightIndex < currentCount && items[swapIndex].CompareTo(items[childRightIndex]) < 0)
                        swapIndex = childRightIndex;

                    if (item.CompareTo(items[swapIndex]) < 0)
                        Swap(items[swapIndex], item);
                    else
                        break;
                }
                else
                    break;
            }
        }

        void SortUp(T item)
        {
            int parentIndex = (int)((item.HeapIndex - 1) * .5f);

            while (true)
            {
                T parentItem = items[parentIndex];
                if (item.CompareTo(parentItem) > 0)
                    Swap(parentItem, item);
                else
                    break;
            }
        }

        void Swap(T itemA, T itemB)
        {
            items[itemA.HeapIndex] = itemB;
            items[itemB.HeapIndex] = itemA;
            int temp = itemA.HeapIndex;
            itemA.HeapIndex = itemB.HeapIndex;
            itemB.HeapIndex = temp;
        }
    }

    public interface IHeapItem<T> : IComparable<T>
    {
        int HeapIndex
        {
            get;
            set;
        }
    }
}