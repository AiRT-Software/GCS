using System.Collections.Generic;

/// <summary>
/// Class used for storing incoming messages from netmq sockets and extract them
/// in the unity's Update function.
/// </summary>
/// <typeparam name="T"></typeparam>
class QueueSync<T>
{
    private Queue<T> _queue = new Queue<T>();
    private object _sync = new object();
    private int max_size;

    public QueueSync(int max_size_)
    {
        max_size = max_size_;
        _queue = new Queue<T>();
    }

    public int GetSize()
    {
        lock (_sync)
        {
            return _queue.Count;
        }
    }

    // adds an object at the END
    public void Enqueue(T value)
    {
        lock (_sync)
        {
            if (_queue.Count == max_size)
            {
                // remove the oldest at the beginning
                _queue.Dequeue();
            }

            // add the new at the end
            _queue.Enqueue(value);
        }
    }

    // removes and returns the object at the beginning
    public T Dequeue()
    {
        lock (_sync)
        {
            return _queue.Dequeue();
        }
    }

    // returns the object at the beginning without removing it
    public T Peek()
    {
        lock (_sync)
        {
            return _queue.Peek();
        }
    }
}

