using System;
using System.Collections.Generic;
using System.Text;

namespace CameraCommon
{
    public class Clip : Capture
    {
        public TimeSpan Duration { get; set; }

        protected override string Extension { get; set; } = "h264";

        public string DataFile { get; set; }
    }
}
