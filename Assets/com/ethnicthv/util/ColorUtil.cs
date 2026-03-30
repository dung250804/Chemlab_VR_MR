using UnityEngine;

public static class ColorUtil
{
    // Color → int (ARGB)
    public static int ToHex(this Color c)
    {
        Color32 c32 = c;
        return (c32.a << 24) | (c32.r << 16) | (c32.g << 8) | c32.b;
    }

    // int → Color
    public static Color GetColor(int hex)
    {
        float a = ((hex >> 24) & 0xFF) / 255f;
        float r = ((hex >> 16) & 0xFF) / 255f;
        float g = ((hex >> 8) & 0xFF) / 255f;
        float b = (hex & 0xFF) / 255f;

        return new Color(r, g, b, a);
    }
}