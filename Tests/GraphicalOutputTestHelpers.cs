using ImageMagick;

namespace Tests
{
    internal static class GraphicalOutputTestHelpers
    {

        public static (int, string, bool) FrameMatchesExpectedFrame(string romPath, string imagePath, string outputFile, int frameToCheck)
        {
            var render = new TestRenderDevice();

            var rom = File.ReadAllBytes(romPath);
            var expectedImage = new MagickImage(imagePath);

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
    }
}