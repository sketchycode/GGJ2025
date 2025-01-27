using UnityEngine;

public static class ColorExtensions
{
    public static Color FromHex(int value)
    {
        return FromHex(value, 1f);
    }
    
    public static Color FromHex(int value, float alpha)
    {
        // Mask and extract individual color components (RGB)
        float r = ((value >> 16) & 0xFF) / 255f;
        float g = ((value >> 8) & 0xFF) / 255f;
        float b = (value & 0xFF) / 255f;

        // Assuming alpha is fully opaque
        return new Color(r, g, b, alpha);
    }
}
