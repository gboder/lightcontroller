using CameraCommon;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Common
{
    public class Context
    {
        public static Context Current
        {
            get
            {
                if (Container == null)
                    throw new InvalidOperationException($"Cannot access context as no IoC container is defined");
                return Container.GetInstance<Context>();
            }
        }
        public static Container Container { get; set; }

        public ICameraManager CameraManager => Container.GetInstance<ICameraManager>();

        public IStoreProvider<StillCapture, int> StillStorage => Container.GetInstance<IStoreProvider<StillCapture, int>>();
    }
}
