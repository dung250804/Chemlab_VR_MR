namespace com.ethnicthv.chemlab.client.api.core.game
{
    public interface IHeater
    {
        public void SetHeatable(IHeatable heatable);
        public bool IsAttachedToHeatable(out IHeatable heatable);
    }
}