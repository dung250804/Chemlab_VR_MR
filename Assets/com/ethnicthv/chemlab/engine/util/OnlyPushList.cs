using System.Collections.Generic;

namespace com.ethnicthv.chemlab.engine.util
{
    public class CustomList<T> : IOnlyPushList<T>
    {
        private LinkedList<T> list = new LinkedList<T>();
        
        public void Push(T item)
        {
            list.AddLast(item);
        }
        
        public LinkedList<T> GetList()
        {
            return list;
        }
        
        public void Clear()
        {
            list.Clear();
        }
    }
}