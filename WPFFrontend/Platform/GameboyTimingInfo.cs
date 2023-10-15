using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFFrontend.Platform;

public partial class GameboyTimingInfo : ObservableObject
{
    private int currentFrame;
    private void AddFrameTimeToQueue()
    {
        FrameTimes[currentFrame++] = DateTime.Now;
        if (currentFrame == 16)
        {
            FrameTime = Elapsed(15, 14).TotalMilliseconds;
            AverageFPS();
            currentFrame = 0;
        }
    }

    private readonly DateTime[] FrameTimes = new DateTime[16];

    private double FrameTime;
    private double GameboyFPS;
    private void AverageFPS()
    {
        TimeSpan totalElapsed = TimeSpan.Zero;
        for (int i = 1; i < FrameTimes.Length; i++)
        {
            totalElapsed += Elapsed(i, i - 1);
        }

        GameboyFPS = TimeSpan.FromSeconds(1) / (totalElapsed / (FrameTimes.Length - 1));
    }
    private TimeSpan Elapsed(int i, int j) => FrameTimes[i] - FrameTimes[j];

    private int frameNumber;
    public void Update()
    {
        AddFrameTimeToQueue();

        PerformanceDisplayText = string.Format("Frame:{0}\t FrameTime:{1:N2}\t FPS:{2:N2}",
             frameNumber++,
             FrameTime,
             GameboyFPS);
    }

    [ObservableProperty]
    public string? performanceDisplayText;
}
