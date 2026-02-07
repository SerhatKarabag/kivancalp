using System;

namespace Kivancalp.Gameplay.Application
{
    internal struct CircularQueue<T> where T : struct
    {
        private readonly T[] _buffer;
        private int _head;
        private int _tail;
        private int _count;

        public CircularQueue(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
            }

            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public int Count => _count;

        public int Capacity => _buffer.Length;

        public bool IsFull => _count >= _buffer.Length;

        public void Enqueue(T value)
        {
            if (_count >= _buffer.Length)
            {
                throw new InvalidOperationException("Queue capacity exceeded.");
            }

            _buffer[_tail] = value;
            _tail += 1;

            if (_tail == _buffer.Length)
            {
                _tail = 0;
            }

            _count += 1;
        }

        public T Dequeue()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            T value = _buffer[_head];
            _head += 1;

            if (_head == _buffer.Length)
            {
                _head = 0;
            }

            _count -= 1;
            return value;
        }

        public T Peek()
        {
            if (_count <= 0)
            {
                throw new InvalidOperationException("Queue is empty.");
            }

            return _buffer[_head];
        }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _count = 0;
        }
    }
}
