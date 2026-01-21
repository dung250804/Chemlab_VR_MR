namespace com.ethnicthv.chemlab.engine.util
{
    public interface IOnlyPushList<in T>
    {
        public void Push(T item);
    }
}