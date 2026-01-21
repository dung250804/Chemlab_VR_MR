using com.ethnicthv.chemlab.engine.api;

namespace com.ethnicthv.chemlab.engine.serializer
{
    public class BondSerialize
    {
        public static string Serialize(Bond.BondType bond)
        {
            return bond switch
            {
                Bond.BondType.Single => "-",
                Bond.BondType.Double => "=",
                Bond.BondType.Triple => "#",
                Bond.BondType.Aromatic => ":",
                _ => ""
            };
        }
        
        public static Bond.BondType Deserialize(char bond)
        {
            return bond switch
            {
                '-' => Bond.BondType.Single,
                '=' => Bond.BondType.Double,
                '#' => Bond.BondType.Triple,
                ':' => Bond.BondType.Aromatic,
                _ => Bond.BondType.Single
            };
        }
    }
}