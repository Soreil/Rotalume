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
        List<byte> bytes = new(40);

        bytes.AddRange(RiffTag);
        bytes.AddRange(BitConverter.GetBytes(36 + SubChunk2Size));
        bytes.AddRange(WaveTag);
        bytes.AddRange(FmtTag);
        bytes.AddRange(BitConverter.GetBytes(FormatDatalength));
        bytes.AddRange(BitConverter.GetBytes((ushort)Format));
        bytes.AddRange(BitConverter.GetBytes(ChannelCount));
        bytes.AddRange(BitConverter.GetBytes(SampleRate));
        bytes.AddRange(BitConverter.GetBytes(SampleRate * ChannelCount * BitsPerSample / 8));
        bytes.AddRange(BitConverter.GetBytes((ushort)(ChannelCount * BitsPerSample / 8)));
        bytes.AddRange(BitConverter.GetBytes(BitsPerSample));
        bytes.AddRange(DatTag);
        bytes.AddRange(BitConverter.GetBytes(SubChunk2Size));

        return [.. bytes];
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
