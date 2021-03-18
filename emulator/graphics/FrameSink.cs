using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace emulator
{
    public class FrameSink
    {
        private readonly byte[] frameData;

        private readonly Func<IntPtr> Lock;
        private readonly Action Unlock;

        private const long timePerFrame = (long)(TimeSpan.TicksPerSecond / ((1 << 22) / 70224.0));
        private readonly Stopwatch stopWatch = new();
        private readonly bool LimitFPS;
        public FrameSink(Func<IntPtr> Lock, Action Unlock, bool LimitFPS)
        {
            frameData = new byte[144 * 160];

            this.Lock = Lock;
            this.Unlock = Unlock;
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

            var ptr = Lock!();
            if (ptr != IntPtr.Zero)
            {
                unsafe
                {
                    Marshal.Copy(frameData, 0, ptr, frameData.Length);
                }
            }
            Unlock!();

            Position = 0;
            OnFramePushed(EventArgs.Empty);
        }

        public void Write(ReadOnlySpan<byte> buffer)
        {
            buffer.CopyTo(new Span<byte>(frameData, Position, buffer.Length));
            Position += buffer.Length;
        }
    }
}
