using System;
using System.Collections.Generic;

namespace com.ethnicthv.util.pool
{
    public class Pool<T> where T : IPoolable
    {
        private readonly Stack<T> _pool = new();
        private readonly Func<T> _factory;

        public Pool(Func<T> factory)
        {
            _factory = factory;
        }

        public T Get()
        {
            return _pool.Count > 0 ? _pool.Pop() : _factory();
        }

        public void Return(T obj)
        {
            obj.ResetInstance();
            _pool.Push(obj);
        }
    }
    
    public class GameObjectPool<T>
    {
        private readonly Queue<T> _pool = new();
        private readonly Func<T> _factory;
        private readonly Action<T> _reset;
        
        public GameObjectPool(Func<T> factory, Action<T> reset)
        {
            _factory = factory;
            _reset = reset;
        }
        
        public T Get()
        {
            return _pool.Count > 0 ? _pool.Dequeue() : _factory();
        }
        
        public void Return(T obj)
        {
            _reset(obj);
            _pool.Enqueue(obj);
        }
    }
}