using emulator.graphics;
using ImageMagick;

namespace Tests
{
    internal static class GraphicalOutputTestHelpers
    {

        public static (int, string, bool) FrameMatchesExpectedFrame(string romPath, string imagePath, string outputFile, int frameToCheck)
        {
            var render = new TestRenderDevice();

            var rom = File.ReadAllBytes(romPath);
            using var expectedImage = new MagickImage(imagePath);
            NormalizeExpectedPalette(expectedImage);


            var core = TestHelpers.NewCore(rom, Path.GetFileNameWithoutExtension(romPath), render);

            int FramesDrawn = 0;
            bool EarlyExitFrameSeen = false;
            render.FramePushed += (sender, e) =>
            {
                //check if we can return early
                if (FramesDrawn % 10 == 0)
                {
                    var settings = new MagickReadSettings
                    {
                        Width = 160,
                        Height = 144,
                        Format = MagickFormat.Gray
                    };
                    using var outputImage = new MagickImage(render.Image, settings);

                    if (ImageComparer.AreImagesEqual(expectedImage, outputImage))
                    {
                        outputImage.Write(outputFile, MagickFormat.Bmp);
                        EarlyExitFrameSeen = true;
                    }
                }
                FramesDrawn++;
            };

            while (FramesDrawn != frameToCheck && !EarlyExitFrameSeen)
                core.Step();
            core.Dispose();

            if (EarlyExitFrameSeen) return (FramesDrawn, outputFile, true);


            var settings = new MagickReadSettings
            {
                Width = 160,
                Height = 144,
                Format = MagickFormat.Gray
            };
            using var outputImage = new MagickImage(render.Image, settings);

            outputImage.Write(outputFile, MagickFormat.Bmp);
            return (FramesDrawn, outputFile, false);
        }

        internal static void NormalizeExpectedPalette(MagickImage image)
        {
            byte[] palette =
            [
                Renderer.ShadeToGray(Shade.White),
                Renderer.ShadeToGray(Shade.LightGray),
                Renderer.ShadeToGray(Shade.DarkGray),
                Renderer.ShadeToGray(Shade.Black)
            ];
            var colors = image.Histogram().Keys
                .OrderByDescending(color => 0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B)
                .ToArray();

            if (colors.All(color => color.R == color.G && color.G == color.B && palette.Contains(color.R)))
                return;

            var mapping = colors.Select((color, index) => new
            {
                Color = color,
                Gray = colors.Length == 1
                    ? palette.MinBy(gray => Math.Abs(gray - (0.2126 * color.R + 0.7152 * color.G + 0.0722 * color.B)))
                    : palette[(int)Math.Round(index * (palette.Length - 1.0) / (colors.Length - 1))]
            }).ToDictionary(entry => entry.Color, entry => entry.Gray);

            image.ColorType = ColorType.TrueColorAlpha;
            using var pixels = image.GetPixels();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var color = pixels.GetPixel(x, y).ToColor();
                    var gray = mapping[color];
                    pixels.SetPixel(x, y, [gray, gray, gray, color.A]);
                }
            }
        }
    }
}