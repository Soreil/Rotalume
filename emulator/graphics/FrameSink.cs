using System;

namespace emulator
{
    public class FrameSink
    {
        private readonly byte[] frameData;
        private int position;
        private readonly Action<byte[]> Render;
        public int FrameCount { get; set; }
        public FrameSink(Action<byte[]> render)
        {
            frameData = new byte[144 * 160];
            position = 0;
            FrameCount = 0;
            Render = render;
            Write = WriteNormal;
        }

        public int Position => position;
        public FrameSink()
        {
            frameData = Array.Empty<byte>();
            position = 0;
            Render = (x) => { };
            Write = WriteEmpty;
        }

        public long Length { get => frameData.Length; }
        public void PushFrame()
        {
            Render(frameData);
            position = 0;
            FrameCount++;
        }
        public delegate void Writer(byte[] buffer);
        public Writer Write;
        private void WriteNormal(byte[] buffer)
        {
            buffer.CopyTo(frameData, position);
            position += buffer.Length;
        }
        private void WriteEmpty(byte[] buffer)
        {
        }
    }
}
