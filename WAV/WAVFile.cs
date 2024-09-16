using System.Runtime.InteropServices;

namespace WAV;

public class WAVFile<T> where T : unmanaged
{
    private static readonly byte[] RiffTag = "RIFF"u8.ToArray();
    private static readonly byte[] WaveTag = "WAVE"u8.ToArray();
    private static readonly byte[] FmtTag = "fmt "u8.ToArray();
    private static readonly byte[] DatTag = "data"u8.ToArray();

    private readonly int FormatDatalength;
    private readonly FormatType Format;
    private readonly ushort ChannelCount;
    private readonly int SampleRate;
    private readonly ushort BitsPerSample;

    private readonly int SubChunk2Size;

    public WAVFile(ReadOnlySpan<T> data, ushort channelCount, int sampleRate, ushort bitsPerSample)
    {
        Format = FormatType.PCM;
        FormatDatalength = 16;

        ChannelCount = channelCount;
        SampleRate = sampleRate;
        BitsPerSample = bitsPerSample;

        unsafe
        {
            SubChunk2Size = data.Length * sizeof(T);
        }
    }

    private byte[] SerializeHeader()
    {
        byte[] header = new byte[44];

        Buffer.BlockCopy(RiffTag, 0, header, 0, 4);
        _ = BitConverter.TryWriteBytes(header.AsSpan(4, 4), 36 + SubChunk2Size);
        Buffer.BlockCopy(WaveTag, 0, header, 8, 4);
        Buffer.BlockCopy(FmtTag, 0, header, 12, 4);
        _ = BitConverter.TryWriteBytes(header.AsSpan(16, 4), FormatDatalength);
        _ = BitConverter.TryWriteBytes(header.AsSpan(20, 2), (ushort)Format);
        _ = BitConverter.TryWriteBytes(header.AsSpan(22, 2), ChannelCount);
        _ = BitConverter.TryWriteBytes(header.AsSpan(24, 4), SampleRate);
        _ = BitConverter.TryWriteBytes(header.AsSpan(28, 4), SampleRate * ChannelCount * BitsPerSample / 8);
        _ = BitConverter.TryWriteBytes(header.AsSpan(32, 2), (ushort)(ChannelCount * BitsPerSample / 8));
        _ = BitConverter.TryWriteBytes(header.AsSpan(34, 2), BitsPerSample);
        Buffer.BlockCopy(DatTag, 0, header, 36, 4);
        _ = BitConverter.TryWriteBytes(header.AsSpan(40, 4), SubChunk2Size);

        return header;
    }

    public void Write(BinaryWriter writer, ReadOnlySpan<T> data)
    {
        var header = SerializeHeader();
        writer.Write(header);
        var bytes = ToBytes(data);
        writer.Write(bytes);
    }

    public static ReadOnlySpan<byte> ToBytes(ReadOnlySpan<T> span) => MemoryMarshal.Cast<T, byte>(span);
}
