namespace com.ethnicthv.chemlab.client.api.core
{
    public interface ITranslator
    {
        public string Translate(string key);
        public void AddTranslation(string key, string value);
        public bool HasTranslation(string key);
    }
}