using UnityEngine;

public class Headarray<T>
{
    private int size;
    private int head;
    private T[] data;

    public Headarray(int size)
    {
        this.size = size;
        head = 0;
        data = new T[size];
    }

    public int getSize()
    {
        return this.size;
    }

    public void addToBot(T newData)
    {
        reduceHead();
        data[head] = newData;
    }
    public void addToTop(T newData)
    {
        int cindex = head == 0 ? size - 1 : head - 1;
        data[cindex] = newData;
    }

    public T get(int index)
    {
        if(index >= size)
        {
            throw new System.IndexOutOfRangeException();
        }
        int cindex = (head + index) % size;
        return data[cindex];
    }

    public void set(int index, T newBlock)
    {
        int cindex = (head + index) % size;
        data[cindex] = newBlock;
    }

    private void reduceHead()
    {
        head = head == 0 ? size - 1 : head - 1;
    }
}
