using emulator.graphics;

using NUnit.Framework;

using WPFFrontend.Platform;

namespace Tests;

internal class FrontendFramePixelsTests
{
    [TestCase(Shade.White)]
    [TestCase(Shade.LightGray)]
    [TestCase(Shade.DarkGray)]
    [TestCase(Shade.Black)]
    public void ConversionPreservesEachInternalShade(Shade shade)
    {
        byte gray = Renderer.ShadeToGray(shade);
        byte[] source = [gray];
        byte[] converted = FramePixels.ToBgra(source, 1, 1);

        Assert.That(converted, Is.EqualTo([gray, gray, gray, 255]));
        Assert.That(source, Is.EqualTo([gray]));
    }

    [Test]
    public void DisplayScalingRepeatsPixelsAndRowsWithoutInterpolation()
    {
        byte[] source = [255, 170, 85, 0];
        byte[] converted = FramePixels.ToBgra(source, 2, 2, FramePixels.DisplayScale);
        int width = 2 * FramePixels.DisplayScale;
        Assert.That(converted, Has.Length.EqualTo(width * width * 4));
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte expected = source[(y / FramePixels.DisplayScale * 2) + (x / FramePixels.DisplayScale)];
                int offset = ((y * width) + x) * 4;
                Assert.That(converted.AsSpan(offset, 4).ToArray(), Is.EqualTo(new byte[] { expected, expected, expected, 255 }));
            }
        }
    }

    [Test]
    public void NativeFrameConversionKeepsScreenshotDimensions()
    {
        byte[] frame = new byte[FramePixels.Width * FramePixels.Height];
        Assert.That(FramePixels.ToBgra(frame, FramePixels.Width, FramePixels.Height),
            Has.Length.EqualTo(160 * 144 * 4));
    }

    [Test]
    public void BlendingAveragesFramesWithoutMutatingEitherInput()
    {
        byte[] current = [255, 170, 85, 0];
        byte[] previous = [0, 85, 170, 255];
        Assert.That(FramePixels.Blend(current, previous), Is.EqualTo(new byte[] { 127, 127, 127, 127 }));
        Assert.That(current, Is.EqualTo(new byte[] { 255, 170, 85, 0 }));
        Assert.That(previous, Is.EqualTo(new byte[] { 0, 85, 170, 255 }));
    }

    [TestCase(0, 1, 1)]
    [TestCase(1, 0, 1)]
    [TestCase(1, 1, 0)]
    [TestCase(-1, 1, 1)]
    [TestCase(1, 1, -1)]
    public void ConversionRejectsInvalidDimensions(int width, int height, int scale)
    {
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => FramePixels.ToBgra(new byte[1], width, height, scale));
    }

    [Test]
    public void ConversionRejectsWrongFrameLength()
    {
        _ = Assert.Throws<ArgumentException>(() => FramePixels.ToBgra(new byte[3], 2, 2));
    }

    [Test]
    public void BlendingRejectsMismatchedFrameLengths()
    {
        _ = Assert.Throws<ArgumentException>(() => FramePixels.Blend(new byte[1], new byte[2]));
    }
}
