using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Common.Tools
{
    public static class Utilities
    {
        public static bool IsRunningOnPi => RuntimeInformation.ProcessArchitecture == Architecture.Arm;
    }
}
