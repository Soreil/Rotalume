using System.IO;
using System;


namespace emulator
{
    public class FrameSink : Stream
    {
        private readonly byte[] frameData;
        private long position;
        private Action<byte[]> Render;
        public int frameCount { get; set; }
        public FrameSink(Action<byte[]> render)
        {
            frameData = new byte[144 * 160];
            position = 0;
            frameCount = 0;
            Render = render;
        }

        public override bool CanRead { get => true; }
        public override bool CanSeek { get => true; }
        public override bool CanWrite { get => true; }
        public override long Length { get => frameData.Length; }
        public override long Position { get => position; set => position = value; }
        public override void Flush()
        {
            Render(frameData);
            position = 0;
            frameCount++;
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            var buf = frameData[offset..(offset + count)];
            buf.CopyTo(buffer, 0);
            return Math.Max(buf.Length, count);
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.End:
                    position = frameData.Length - offset - 1;
                    break;
            }
            return position;
        }
        public override void SetLength(long value) => throw new System.NotImplementedException();
        public override void Write(byte[] buffer, int offset, int count)
        {
            var slice = buffer[offset..(offset + count)];
            slice.CopyTo(frameData, position);
            position += count;
        }
    }
}
