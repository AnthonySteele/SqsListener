using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqsListener
{
    /// <summary>
    /// This is not thread-safe, but it does not need to be:
    /// it should be owned by 1 SimpleListener
    /// And accessed only thereby, in a serial fashion
    /// </summary>
    public class TaskList
    {
        private readonly int _maxSize;
        private readonly List<Task> _running;

        public TaskList(int maxSize)
        {
            _maxSize = maxSize;
            _running = new List<Task>(_maxSize);
        }

        public int Capacity => _maxSize - _running.Count;

        public void Add(Task t)
        {
            if (Capacity <= 0)
            {
                throw new InvalidOperationException("Task list is full");
            }

            _running.Add(t);
        }

        public async Task WhenAny()
        {
            if (_running.Count == 0)
            {
                return;
            }

            await Task.WhenAny(_running);
        }

        public int ClearCompleted()
        {
            var removed = 0;

            for (var index = _running.Count - 1; index >= 0; index--)
            {
                if (_running[index].IsCompleted)
                {
                    _running.RemoveAt(index);
                    removed++;
                }
            }

            return removed;
        }
    }
}
