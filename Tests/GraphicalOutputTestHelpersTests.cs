using ImageMagick;
using NUnit.Framework;

namespace Tests;

internal class GraphicalOutputTestHelpersTests
{
    [Test]
    public void FourThemedColorsMapByBrightnessRank()
    {
        using var image = CreateImage("#081820", "#88c070", "#e0f8d0", "#346856");

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        AssertPixels(image, 0, 192, 255, 64);
    }

    [Test]
    public void TwoThemedColorsMapToPaletteEndpoints()
    {
        using var image = CreateImage("#081820", "#e0f8d0", "#081820");

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        AssertPixels(image, 0, 255, 0);
    }

    [Test]
    public void InternalGrayscalePaletteRemainsUnchanged()
    {
        using var image = CreateImage("#404040", "#ffffff", "#000000", "#c0c0c0");

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        AssertPixels(image, 64, 255, 0, 192);
    }

    [Test]
    public void InternalGrayscaleSubsetRemainsUnchanged()
    {
        using var image = CreateImage("#404040", "#c0c0c0");

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        AssertPixels(image, 64, 192);
    }

    [Test]
    public void NormalizedImageMatchesRawRendererGrayscale()
    {
        using var image = CreateImage("#e0f8d0", "#88c070", "#346856", "#081820");
        using var expected = new MagickImage(new byte[] { 255, 192, 64, 0 }, new MagickReadSettings
        {
            Width = 4,
            Height = 1,
            Format = MagickFormat.Gray
        });

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        Assert.That(ImageComparer.AreImagesEqual(image, expected), Is.True);
    }

    [TestCase("01-registers")]
    [TestCase("02-len_ctr")]
    [TestCase("03-trigger")]
    [TestCase("04-sweep")]
    [TestCase("05-sweep_details")]
    [TestCase("06-overflow_on_trigger")]
    [TestCase("07-len_sweep_period_sync")]
    [TestCase("08-len_ctr_during_power")]
    [TestCase("09-wave_read_while_on")]
    [TestCase("10-wave_trigger_while_on")]
    [TestCase("11-regs_after_power")]
    [TestCase("12-wave_write_while_on")]
    public void DmgSoundReferenceUsesGrayscaleEndpoints(string name)
    {
        using var image = new MagickImage($@"rom\blargg\dmg_sound\{name}.png");
        uint width = image.Width;
        uint height = image.Height;

        GraphicalOutputTestHelpers.NormalizeExpectedPalette(image);

        var colors = image.Histogram().Keys.ToArray();
        Assert.That(image.Width, Is.EqualTo(width));
        Assert.That(image.Height, Is.EqualTo(height));
        Assert.That(colors, Has.Length.EqualTo(2));
        Assert.That(colors.All(color => color.R == color.G && color.G == color.B && color.R is 0 or 255), Is.True);
    }

    private static MagickImage CreateImage(params string[] colors)
    {
        var image = new MagickImage(MagickColors.White, (uint)colors.Length, 1);
        image.ColorType = ColorType.TrueColorAlpha;
        using var pixels = image.GetPixels();
        for (int x = 0; x < colors.Length; x++)
        {
            var color = new MagickColor(colors[x]);
            pixels.SetPixel(x, 0, [color.R, color.G, color.B, color.A]);
        }
        return image;
    }

    private static void AssertPixels(MagickImage image, params byte[] expected)
    {
        using var pixels = image.GetPixels();
        for (int x = 0; x < expected.Length; x++)
        {
            var color = pixels.GetPixel(x, 0).ToColor();
            Assert.That(color.R, Is.EqualTo(expected[x]), $"Red at pixel {x}");
            Assert.That(color.G, Is.EqualTo(expected[x]), $"Green at pixel {x}");
            Assert.That(color.B, Is.EqualTo(expected[x]), $"Blue at pixel {x}");
        }
    }
}
