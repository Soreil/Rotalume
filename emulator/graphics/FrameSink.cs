using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace emulator
{
    public class FrameSink
    {
        private readonly byte[] frameData;


        private readonly Action? Lock;
        private readonly Action? Unlock;
        private readonly IntPtr Pointer = IntPtr.Zero;

        private readonly long timePerFrame;
        private readonly Stopwatch stopWatch = new();
        private readonly bool LimitFPS;
        public FrameSink(Action Lock, Action Unlock, IntPtr Pointer, bool LimitFPS)
        {
            frameData = new byte[144 * 160];
            Position = 0;

            this.Lock = Lock;
            this.Unlock = Unlock;
            this.Pointer = Pointer;

            timePerFrame = (TimeSpan.FromSeconds(1) / ((1 << 22) / 70224.0)).Ticks;
            this.LimitFPS = LimitFPS;

            stopWatch.Start();
        }

        public int Position { get; private set; }

        protected virtual void OnFramePushed(EventArgs e)
        {
            if (FramePushed is not null)
                FramePushed(this, e);
        }

        public event EventHandler? FramePushed;

        public void Draw()
        {
            if (LimitFPS)
            {
                while (stopWatch.ElapsedTicks < timePerFrame)
                {

                }
            }
            stopWatch.Restart();

            Lock!();
            unsafe
            {
                Marshal.Copy(frameData, 0, Pointer, frameData.Length);
            }
            Unlock!();

            Position = 0;
            OnFramePushed(EventArgs.Empty);
        }

        public void Write(byte[] buffer)
        {
            buffer.CopyTo(frameData, Position);
            Position += buffer.Length;
        }
    }
}
