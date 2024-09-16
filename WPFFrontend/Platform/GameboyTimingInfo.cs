using CommunityToolkit.Mvvm.ComponentModel;

namespace WPFFrontend.Platform;

public partial class GameboyTimingInfo : ObservableObject
{
    private int currentFrame;
    private readonly DateTime[] frameTimes = new DateTime[16];
    private double frameTime;
    private double gameboyFPS;
    private int frameNumber;

    private void AddFrameTimeToQueue()
    {
        frameTimes[currentFrame++] = DateTime.Now;
        if (currentFrame == 16)
        {
            frameTime = Elapsed(15, 14).TotalMilliseconds;
            CalculateAverageFPS();
            currentFrame = 0;
        }
    }

    private void CalculateAverageFPS()
    {
        TimeSpan totalElapsed = TimeSpan.Zero;
        for (int i = 1; i < frameTimes.Length; i++)
        {
            totalElapsed += Elapsed(i, i - 1);
        }

        gameboyFPS = TimeSpan.FromSeconds(1) / (totalElapsed / (frameTimes.Length - 1));
    }
    private TimeSpan Elapsed(int i, int j) => frameTimes[i] - frameTimes[j];

    public void Update()
    {
        AddFrameTimeToQueue();

        PerformanceDisplayText = $"Frame:{frameNumber++}\t FrameTime:{frameTime:N2}\t FPS:{gameboyFPS:N2}";
    }

    [ObservableProperty]
    public string? performanceDisplayText;
}
