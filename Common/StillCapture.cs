namespace CameraCommon
{
    public class StillCapture : Capture
    {
        public const string StillExtension = "jpg";
        protected override string Extension { get; set; } = StillExtension;
        public virtual byte[] Data { get; set; }
    }
}