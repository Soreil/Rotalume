using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
        private readonly bool LimitFPS;
        private readonly StreamWriter Logger;
        public FrameSink(Action Lock, Action Unlock, IntPtr Pointer, bool LimitFPS)
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
            this.LimitFPS = LimitFPS;

            if (LimitFPS)
            {
                var logPath = "frametimes.txt";
                if (!File.Exists(logPath))
                    Logger = File.CreateText(logPath);
                else Logger = new StreamWriter(File.Open(logPath, FileMode.Truncate, FileAccess.Write, FileShare.Read));
            }
            else Logger = null;

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
            if (LimitFPS)
            {
                while (stopWatch.ElapsedTicks < timePerFrame)
                {

                }
                var spent = stopWatch.ElapsedTicks;
                if (spent > timePerFrame * 2) Debugger.Break();
                Logger.WriteLineAsync(spent.ToString());
            }
            stopWatch.Restart();

            new Task(() =>
            {
                Lock();
                unsafe
                {
                    Marshal.Copy(frameData, 0, Pointer, frameData.Length);
                }
                Unlock();
            }).Start();
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
