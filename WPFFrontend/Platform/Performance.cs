using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace WPFFrontend;

public partial class Performance : ObservableObject
{
    private int currentFrame;
    private void AddFrameTimeToQueue()
    {
        FrameTimes[currentFrame++] = DateTime.Now;
        if (currentFrame == 16)
        {
            FrameTime = Delta(15, 14).TotalMilliseconds;
            AverageFPS();
            currentFrame = 0;
        }
    }

    private readonly DateTime[] FrameTimes = new DateTime[16];

    private double FrameTime;
    private double GameboyFPS;
    private void AverageFPS()
    {
        TimeSpan deltas = TimeSpan.Zero;
        for (int i = 1; i < FrameTimes.Length; i++)
        {
            deltas += Delta(i, i - 1);
        }

        GameboyFPS = TimeSpan.FromSeconds(1) / (deltas / (FrameTimes.Length - 1));
    }
    private TimeSpan Delta(int i, int j) => FrameTimes[i] - FrameTimes[j];

    private int frameNumber;
    public void Update()
    {
        AddFrameTimeToQueue();

        Label = string.Format("Frame:{0}\t FrameTime:{1:N2}\t FPS:{2:N2}",
             frameNumber++,
             FrameTime,
             GameboyFPS);
    }

    [ObservableProperty]
    public string? label;
}
