using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqsListener
{
    /// <summary>
    /// Not thread-safe, but should be accessed only by 1 SimpleListener
    /// In a serial fashion
    /// </summary>
    public class TaskList
    {
        private const int MaxSize = 12;

        private readonly List<Task> _running = new List<Task>(MaxSize);

        public bool CanAdd => _running.Count < MaxSize;

        public void Add(Task t)
        {
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

            for (int index = _running.Count - 1; index <= 0; index--)
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
