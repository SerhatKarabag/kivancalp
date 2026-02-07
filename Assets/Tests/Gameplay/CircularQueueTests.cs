using System;
using Kivancalp.Gameplay.Application;
using NUnit.Framework;

namespace Kivancalp.Gameplay.Tests
{
    [TestFixture]
    public sealed class CircularQueueTests
    {
        [Test]
        public void EnqueueDequeue_FifoOrder()
        {
            var queue = new CircularQueue<int>(4);

            queue.Enqueue(10);
            queue.Enqueue(20);
            queue.Enqueue(30);

            Assert.AreEqual(10, queue.Dequeue());
            Assert.AreEqual(20, queue.Dequeue());
            Assert.AreEqual(30, queue.Dequeue());
        }

        [Test]
        public void Peek_ReturnsValueWithoutRemoving()
        {
            var queue = new CircularQueue<int>(4);
            queue.Enqueue(42);

            int peeked = queue.Peek();

            Assert.AreEqual(42, peeked);
            Assert.AreEqual(1, queue.Count);
        }

        [Test]
        public void Wraparound_HeadAndTailWrapCorrectly()
        {
            var queue = new CircularQueue<int>(3);

            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);
            queue.Dequeue();
            queue.Dequeue();

            queue.Enqueue(4);
            queue.Enqueue(5);

            Assert.AreEqual(3, queue.Dequeue());
            Assert.AreEqual(4, queue.Dequeue());
            Assert.AreEqual(5, queue.Dequeue());
        }

        [Test]
        public void Enqueue_WhenFull_ThrowsInvalidOperationException()
        {
            var queue = new CircularQueue<int>(2);
            queue.Enqueue(1);
            queue.Enqueue(2);

            Assert.Throws<InvalidOperationException>(() => queue.Enqueue(3));
        }

        [Test]
        public void Dequeue_WhenEmpty_ThrowsInvalidOperationException()
        {
            var queue = new CircularQueue<int>(4);

            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        [Test]
        public void Peek_WhenEmpty_ThrowsInvalidOperationException()
        {
            var queue = new CircularQueue<int>(4);

            Assert.Throws<InvalidOperationException>(() => queue.Peek());
        }

        [Test]
        public void Clear_ResetsCount()
        {
            var queue = new CircularQueue<int>(4);
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
        }

        [Test]
        public void IsFull_ReturnsTrueWhenCapacityReached()
        {
            var queue = new CircularQueue<int>(2);

            Assert.IsFalse(queue.IsFull);

            queue.Enqueue(1);
            Assert.IsFalse(queue.IsFull);

            queue.Enqueue(2);
            Assert.IsTrue(queue.IsFull);
        }

        [Test]
        public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularQueue<int>(0));
        }

        [Test]
        public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CircularQueue<int>(-1));
        }
    }
}
