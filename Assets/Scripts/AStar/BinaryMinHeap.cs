using System;

public class BinaryMinHeap<T> where T : IHeapItem<T>
{
    public T[] items;
    private int itemCount;

    public BinaryMinHeap(int maxHeapSize)
    {
        items = new T[maxHeapSize];
    }

    public void AddItemToEndOfHeap(T item)
    {
        item.HeapIndex = itemCount;
        items[itemCount] = item;
        SortItemsUp(item);
        itemCount++;
    }

    public T RemoveFirstItemFromHeap()
    {
        T firstItem = items[0];
        itemCount--;
        items[0] = items[itemCount];
        items[0].HeapIndex = 0;
        SortItemsDown(items[0]);
        return firstItem;
    }

    public int Count
    {
        get
        {
            return itemCount;
        }
    }

    public bool ContainsItem(T item)
    {
        return Equals(items[item.HeapIndex], item);
    }

    public void UpdateItem(T item)
    {
        SortItemsUp(item);
    }

    private void SortItemsDown(T item)
    {
        while (true)
        {
            int childIndexLeft = item.HeapIndex * 2 + 1;
            int childIndexRight = item.HeapIndex * 2 + 2;
            int swapIndex = 0;

            if (childIndexLeft < itemCount)
            {
                swapIndex = childIndexLeft;

                if (childIndexRight < itemCount)
                {
                    if (items[childIndexLeft].CompareTo(items[childIndexRight]) < 0)
                    {
                        swapIndex = childIndexRight;
                    }
                }

                if (item.CompareTo(items[swapIndex]) < 0)
                {
                    SwapItems(item, items[swapIndex]);
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }

    private void SortItemsUp(T item)
    {
        int parentIndex = (item.HeapIndex - 1) / 2;

        while (true)
        {
            T parentItem = items[parentIndex];
            if (item.CompareTo(parentItem) > 0)
            {
                SwapItems(item, parentItem);
            }
            else
            {
                break;
            }

            parentIndex = (item.HeapIndex - 1) / 2;
        }
    }

    private void SwapItems(T itemA, T itemB)
    {
        items[itemA.HeapIndex] = itemB;
        items[itemB.HeapIndex] = itemA;
        int itemAIndex = itemA.HeapIndex;
        itemA.HeapIndex = itemB.HeapIndex;
        itemB.HeapIndex = itemAIndex;
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