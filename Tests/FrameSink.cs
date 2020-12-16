using System.IO;
using System;


namespace Tests
{
    class FrameSink : Stream
    {
        private readonly byte[] frameData;
        private long position;
        DirectoryInfo output;
        int frameCount = 0;

        public FrameSink()
        {
            output = Directory.CreateDirectory("frames");
            frameData = new byte[144 * 160];
            position = 0;
        }
        public override bool CanRead { get => true; }
        public override bool CanSeek { get => true; }
        public override bool CanWrite { get => true; }
        public override long Length { get => frameData.Length; }
        public override long Position { get => position; set => position = value; }

        //We ought to not block while writing out a file, probably best to copy frameData and write that out Async so we can keep creating new ones and
        //not have a race condition if writing out is slow.
        public async override void Flush()
        {
            byte[] tmp = (byte[])frameData.Clone();
            using (var file = File.Create(output + "\\" + "Frame" + frameCount.ToString()))
            {
                frameCount++;
                position = 0;
                await file.WriteAsync(tmp);
            }

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
            buffer[offset..(offset + count)].CopyTo(frameData, position);
            position += count;
        }
    }
}
