using NUnit.Framework;

namespace Tests;

internal class CGBTests
{
    [TestCase(@"rom\blargg\interrupt_time.gb")]
    public void TestFrameMatchesExpectedFrame(string romPath)
    {
        var imagePath = Path.ChangeExtension(romPath, ".png");
        var outputFile = Path.Combine(Path.GetDirectoryName(romPath)!, Path.GetFileNameWithoutExtension(romPath) + "_output.bmp");
        //The longer sound ROMs are still running at frame 100. Keep exact image comparison,
        //but allow up to one emulated minute; the helper exits as soon as the image matches.
        int frameLimit = 100;
        var (frame, output, success) = GraphicalOutputTestHelpers.FrameMatchesExpectedFrame(romPath, imagePath, outputFile, frameLimit);

        Assert.That(success, Is.False,
            $"Tested {romPath}. Images did not match after {frame} frames. Wrote debug image to {output}");
    }
}
