namespace WPFFrontend.Platform;

/// <summary>Converts emulator grayscale bytes without changing their palette.</summary>
internal static class FramePixels
{
    public const int Width = 160;
    public const int Height = 144;
    public const int DisplayScale = 4;

    public static byte[] ToBgra(ReadOnlySpan<byte> grayscale, int width, int height, int scale = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scale);
        if (grayscale.Length != checked(width * height))
            throw new ArgumentException("Expected one grayscale byte per pixel.", nameof(grayscale));

        int scaledWidth = checked(width * scale);
        int scaledHeight = checked(height * scale);
        var output = new byte[checked(scaledWidth * scaledHeight * 4)];
        for (int y = 0; y < scaledHeight; y++)
        {
            for (int x = 0; x < scaledWidth; x++)
            {
                byte shade = grayscale[(y / scale * width) + (x / scale)];
                int offset = ((y * scaledWidth) + x) * 4;
                output[offset] = shade;
                output[offset + 1] = shade;
                output[offset + 2] = shade;
                output[offset + 3] = 255;
            }
        }
        return output;
    }

    public static byte[] Blend(ReadOnlySpan<byte> current, ReadOnlySpan<byte> previous)
    {
        if (current.Length != previous.Length)
            throw new ArgumentException("Frames must have the same dimensions.", nameof(previous));

        var blended = new byte[current.Length];
        for (int i = 0; i < current.Length; i++)
            blended[i] = (byte)((current[i] + previous[i]) / 2);
        return blended;
    }
}
