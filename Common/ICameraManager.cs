using CameraCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ICameraManager
    {
        Task<StillCapture> TakeAsync();

        Task<StillCapture> TakeAndSaveCurrent();
        Task<StillCapture> TakeAndSaveCurrent(ImageResolution imageResolution);

        StillCapture Current { get; }

        Task<StillCapture> TakeAsync(ushort width, ushort height);

        Task<StillCapture> TakeAsync(ImageResolution imageResolution);

        Task<Clip> RecordAsync(TimeSpan duration);
        Task<IReadOnlyCollection<StillCapture>> ListAsync(DateTime day);

        Task<IReadOnlyCollection<StillCapture>> ListAsync();


    }
}
