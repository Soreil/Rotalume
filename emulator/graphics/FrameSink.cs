using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace emulator
{
    public class FrameSink
    {
        private readonly byte[] frameData;
        private int position;
        public int FrameCount { get; private set; }

        private readonly Action Lock;
        private readonly Action Unlock;
        private readonly IntPtr Pointer = IntPtr.Zero;

        private readonly long timePerFrame;
        private readonly Stopwatch stopWatch = new();
        public FrameSink(Action Lock, Action Unlock, IntPtr Pointer)
        {
            frameData = new byte[144 * 160];
            position = 0;
            FrameCount = 0;

            this.Lock = Lock;
            this.Unlock = Unlock;
            this.Pointer = Pointer;

            Write = WriteNormal;
            Draw = PushFrame;

            timePerFrame = (TimeSpan.FromSeconds(1) / ((1 << 22) / 70224.0)).Ticks;
            stopWatch.Start();
        }

        public int Position => position;
        public FrameSink()
        {
            frameData = Array.Empty<byte>();
            position = 0;
            Write = WriteEmpty;
            Draw = DrawEmpty;
        }

        private void DrawEmpty()
        {
        }

        public long Length { get => frameData.Length; }
        private void PushFrame()
        {
            while (stopWatch.ElapsedTicks < timePerFrame)
            {

            }
            stopWatch.Restart();
            Lock();

            unsafe
            {
                Marshal.Copy(frameData, 0, Pointer, frameData.Length);
            }

            Unlock();

            position = 0;
            FrameCount++;
        }
        public delegate void Writer(byte[] buffer);
        public Writer Write;
        public delegate void Drawer();
        public Drawer Draw;
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
