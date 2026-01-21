using UnityEngine;

namespace com.ethnicthv.util
{
    public class ColorUtil
    {
        public static Color GetColor(int color)
        {
            var a = (byte)((color >> 24) & 0xFF);
            var r = (byte)((color >> 16) & 0xFF);
            var g = (byte)((color >> 8) & 0xFF);
            var b = (byte)(color & 0xFF);
            
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }
    }
}