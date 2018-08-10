using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace gbo.api.native
{
    public class I2cDriver : IDisposable
    {
        protected int I2cBusHndle { get; private set; }
        protected byte DeviceAddress { get; private set; }
        private int _openMode;
        private string _i2cFileName;
        private const string _i2cDirectoryName = "/dev";
        private ILogger _logger;

        protected string I2cDeviceFullPath => Path.Combine(_i2cDirectoryName, _i2cFileName);

        protected const int IOCTL_I2C_SLAVE_MODE = 0x0703;
        public const int ReadWriteOpenMode = 2;

        public I2cDriver(string fileName, int mode, byte deviceAddress, ILogger logger)
        {
            DeviceAddress = deviceAddress;
            _i2cFileName = fileName;
            _openMode = mode;
            _logger = logger;
        }
        #region Native methods
        [DllImport("libc.so.6", EntryPoint = "open")]
        public static extern int OpenNative(string fileName, int mode);

        [DllImport("libc.so.6", EntryPoint = "ioctl", SetLastError = true)]
        internal extern static int IoctlNative(int fd, int request, int data);

        internal static I2cDriver CreateNew(string fileName, int mode, byte deviceAddress, ILogger logger)
        {
            var instance = new I2cDriver(fileName, mode, deviceAddress, logger);
            instance.Initialize();
            return instance;
        }

        [DllImport("libc.so.6", EntryPoint = "read", SetLastError = true)]
        internal static extern IntPtr ReadNative(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
        internal static extern IntPtr WriteNative(int handle, byte[] data, int length);

        [DllImport("libc.so.6", EntryPoint = "close", SetLastError = true)]
        private static extern int CloseNative(int busHandle);
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (I2cBusHndle > 0)
                    {
                        CloseNative(I2cBusHndle);
                    }
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        void IDisposable.Dispose()
        {
            Dispose(true);
        }
        #endregion

        public virtual void Write(byte data)
        {
            Write(new byte[] { data }, 1);
        }
        public virtual void Write(byte[] data)
        {
            Write(data, data.Length);
        }
        public virtual void Write(byte[] data, int length)
        {

            if (data.Length == 0 || length == 0)
                return;

            var result = WriteNative(I2cBusHndle, data, length);

            if (result.ToInt32() != length)
            {
                _logger.LogError($"Failed to write #{data.Length} bytes ({Convert.ToBase64String(data)}) to I2cDeviceFullPath (Hndl: {I2cBusHndle}) returned {result}.");
                throw new IOException($"Unable to write the {length} bytes ou of {string.Join(",", data.Select(b => b.ToString("X")))}. Method returned: {result}");
            }
            else
            {
                _logger.LogInformation($"Successfuly wrote #{data.Length} bytes ({Convert.ToBase64String(data)}) to I2cDeviceFullPath (Hndl: {I2cBusHndle}) returned {result}.");
            }
        }

        internal virtual void CtlI2c()
        {
            var result = IoctlNative(I2cBusHndle, IOCTL_I2C_SLAVE_MODE, DeviceAddress);

            if (result < 0)
            {
                _logger.LogError($"Unable to send command {IOCTL_I2C_SLAVE_MODE.ToString("X")} to device at address {DeviceAddress.ToString("X")} using handle {I2cBusHndle}.");
                throw new InvalidOperationException($"Unable to communicate with I2c device at address {DeviceAddress.ToString("X")} with handle {I2cBusHndle} and mode {IOCTL_I2C_SLAVE_MODE.ToString("X")}. Ioctl returned: {result}");
            }
            else
            {
                _logger.LogInformation($"Sent command {IOCTL_I2C_SLAVE_MODE.ToString("X")} to device at address {DeviceAddress.ToString("X")} using handle {I2cBusHndle}.");
            }
        }

        public virtual byte Read()
        {
            return Read(1)[0];
        }
        public virtual byte[] Read(int length)
        {
            var data = new byte[length];

            var result = ReadNative(I2cBusHndle, data, length).ToInt32();
            if (result != length)
            {
                _logger.LogError($"Unable to read from device {length} bytes at address {DeviceAddress.ToString("X")} using handle {I2cBusHndle}.");
                throw new InvalidOperationException($"Unable to read {length} byte(s) from {I2cDeviceFullPath}. Read only {result}");
            }
            else
            {
                _logger.LogInformation($"Unable to read from device {length} bytes at address {DeviceAddress.ToString("X")} using handle {I2cBusHndle}.");
            }

            return data;
        }

        public virtual void Initialize()
        {
            if (disposedValue)
                throw new InvalidProgramException($"Unable to initialize a disposed instance.");

            if (string.IsNullOrWhiteSpace(_i2cFileName) || !File.Exists(I2cDeviceFullPath))
            {
                throw new InvalidOperationException($"Unable to find I2c device {I2cDeviceFullPath}");
            }
            if (DeviceAddress <= 0x0)
            {
                throw new ArgumentOutOfRangeException($"Given i2c device address {DeviceAddress} is not valid.");
            }

            // Tries to open
            I2cBusHndle = GetI2cBusHandle();


            // Tries to init i2c
            CtlI2c();
        }

        internal int GetI2cBusHandle()
        {
            if (I2cBusHndle == 0)
            {
                var result = OpenNative(I2cDeviceFullPath, _openMode);
                if (result < 0)
                {
                    _logger.LogError($"Unable to get i2c bus handle for path {I2cDeviceFullPath} and open mode {_openMode}. Returned code {result}");
                    throw new IOException($"Unable to open i2c device on {I2cDeviceFullPath} with mode {_openMode}. Native open returned: {result}.");
                }
                else
                {
                    _logger.LogInformation($"Got i2c bus handle {result} for path {I2cDeviceFullPath} and open mode {_openMode}.");
                }
                return result;
            }
            else
                return I2cBusHndle;
        }
    }
}