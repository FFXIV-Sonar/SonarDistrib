using Lumina.Data.Files;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using System.Diagnostics;
using SixLabors.ImageSharp.Processing;

namespace SonarResources
{
    public static class TexFileExtensions
    {
        public static Image<Bgra32> ToImage(this TexFile tex)
        {
            return Image.LoadPixelData<Bgra32>(tex.ImageData, tex.Header.Width, tex.Header.Height);
        }

        public static Image<Bgra32> MultiplyWith(this Image<Bgra32> destination, Image<Bgra32> source, Point? point = null)
        {
            point ??= new(0, 0);
            var image = destination.Clone();
            image.Mutate(new DrawImageProcessor(source, point.Value, PixelColorBlendingMode.Multiply, PixelAlphaCompositionMode.SrcOver, 1.0f));
            return image;
        }
    }
}
