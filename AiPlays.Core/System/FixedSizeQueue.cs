namespace AiPlays.Core.System
{
    public class FixedSizeQueue<T>
    {
        public Queue<T> Queue { get; init; }

        public int Size { get; private init; }
        
        public FixedSizeQueue(int size)
        {
            Size = size;
            Queue = new Queue<T>();
        }
        public void Enqueue(T item)
        {
            Queue.Enqueue(item);

            while (Queue.Count > Size)
            {
                Queue.Dequeue();
            }
           
        }

        public Queue<T> GetQueue => Queue;
    }
}
