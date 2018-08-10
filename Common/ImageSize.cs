using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public enum ImageResolution : ushort
    {
        Undefined = 0,
        Small = 10,
        Medium = 50,
        Large = 100,
    }

    public class ImageSize
    {
        public static readonly ImageSize Small = new ImageSize { Width = 328, Height = 246 };
        public static readonly ImageSize Medium = new ImageSize { Width = 1640, Height = 1232 };
        public static readonly ImageSize Large = new ImageSize { Width = 3280, Height = 2464 };

        public ushort Width { get; set; }
        public ushort Height { get; set; }
    }

    public static class ImageSizeUtilities
    {
        public static ImageSize ToDimension(this ImageResolution imageSizeKind)
        {
            switch (imageSizeKind)
            {
                default:
                case ImageResolution.Undefined:
                    throw new NotSupportedException($"Value {imageSizeKind} cannot be transformed into a valid instance of {nameof(ImageSize)}");
                case ImageResolution.Small:
                    return ImageSize.Small;
                case ImageResolution.Medium:
                    return ImageSize.Medium;
                case ImageResolution.Large:
                    return ImageSize.Large;
            }
        }
    }
}
