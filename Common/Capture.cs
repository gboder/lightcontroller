using Common;
using System;
using System.IO;

namespace CameraCommon
{
    public abstract class Capture : EntityBase
    {
        public const string FileNamePrefix = "picam";
        public const string TakenOnFormat = "ddMMyyyy-hhmmss.fff";
        public ushort Width { get; set; }
        public ushort Heigth { get; set; }
        protected virtual string Extension { get; set; }

        public DateTime CapturedOn { get; set; }
        public string FileName => $"{FileNamePrefix}-{CapturedOn.ToString(TakenOnFormat)}.{Extension}";

        public string StoredFileName { get; set; }
    }
}