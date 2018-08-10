using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using gbo.api.extensions;
using gbo.api.native;
using Microsoft.Extensions.Logging;

namespace RelayWebApi.Drivers
{
    public class RelayBoardDriver : IRelayBoardDriver, IInitializable
    {
        private ILogger _logger;
        private bool _isInitialized;
        public ushort RelayCount { get; private set; } = 4;

        private readonly Dictionary<ushort, int> _currentState = new Dictionary<ushort, int>();

        public RelayBoardDriver(ILogger logger)
        {
            _logger = logger;
        }

        protected virtual void InitializeRelayState()
        {
            for (ushort i = 0; i < 4; i++)
            {
                _currentState.Add(i, 0x0);
            }
            Write2Output();
        }

        internal IEnumerable<byte> ReloadState()
        {
            using (var device = GetDriverInstance())
            {
                device.Write(0x2);
                var data = device.Read(2);
                return data;
            }
        }

        private I2cDriver GetDriverInstance()
        {
            return I2cDriver.CreateNew("i2c-1", I2cDriver.ReadWriteOpenMode, 0x20, _logger);
        }

        private void SetOutputMode()
        {
            using (var device = GetDriverInstance())
            {
                device.Write(new byte[] { 0x6, 0xf0 }); // 6th register, set to normal 1/0 output
            }
        }

        public void TurnOff(IEnumerable<ushort> relays) => relays.Run4All(TurnOff);
        public void TurnOff(ushort relay)
        {
            _currentState[relay] = 0;
            Write2Output();
        }
        public void TurnOn(IEnumerable<ushort> relays) => relays.Run4All(TurnOn);
        public void TurnOn(ushort relay)
        {
            _currentState[relay] = 1;
            Write2Output();
        }
        public int GetState(ushort relay)
        {
            return _currentState[relay];
        }
        public IEnumerable<int> GetState()
        {
            return _currentState.Values;
        }
        private void Write2Output()
        {
            var data = _currentState[0] > 0 ? 0b1 : 0b0;
            data |= _currentState[1] > 0 ? 0b10 : 0b0;
            data |= _currentState[2] > 0 ? 0b100 : 0b0;
            data |= _currentState[3] > 0 ? 0b1000 : 0b0;

            _logger.LogInformation($"Write {data.ToHexString()} - {data.ToBinaryString()} to i2c device.");

            using (var device = GetDriverInstance())
                device.Write(new byte[] { 0x2, Convert.ToByte(data) });
        }
        public byte[] MyReadBytes(byte registerAddress, int startAddress, int size)
        {
            using (var device = GetDriverInstance())
            {
                var hndle = device.GetI2cBusHandle();
                if (I2cDriver.IoctlNative(hndle, 0x0703, 0x20) < 0)
                    throw new InvalidOperationException($"Unable to control device ");

                // Write address
                var wData = new byte[] { registerAddress };
                var wresult = I2cDriver.WriteNative(hndle, wData, wData.Length).ToInt32();
                if (wresult != wData.Length)
                    throw new InvalidOperationException($"Unable to write 0x{registerAddress.ToString("X")} to device. Write returned {wresult} and {wData.Length} was expected.");

                var data = new byte[size];
                var result = (I2cDriver.ReadNative(hndle, data, size)).ToInt32();
                if (result != size)
                    throw new InvalidOperationException($"Read {result} instead of {size}");

                return data;
            }
        }

        [Obsolete()]
        private byte[] TurnOnAll()
        {
            using (var device = GetDriverInstance())
            {
                var hndle = device.GetI2cBusHandle();
                if (I2cDriver.IoctlNative(hndle, 0x0703, 0x20) < 0)
                    throw new InvalidOperationException($"Unable to control device ");

                // Write address
                var wData = new byte[] { 0x2, 0x0 };
                var wresult = I2cDriver.WriteNative(hndle, wData, wData.Length).ToInt32();
                if (wresult != wData.Length)
                    throw new InvalidOperationException($"Unable to write 0x{0x2.ToString("X")} to device. Write returned {wresult} and {wData.Length} was expected.");

                // Change mode
                wData = new byte[] { 0x6, 0xf0 };
                wresult = I2cDriver.WriteNative(hndle, wData, wData.Length).ToInt32();
                if (wresult != wData.Length)
                    throw new InvalidOperationException($"Unable to write 0x{0x6.ToString("X")} to device. Write returned {wresult} and {wData.Length} was expected.");

                // Write address
                wData = new byte[] { 0x2, 0xf };
                wresult = I2cDriver.WriteNative(hndle, wData, wData.Length).ToInt32();
                if (wresult != wData.Length)
                    throw new InvalidOperationException($"Unable to write 0x{0x2.ToString("X")} to device. Write returned {wresult} and {wData.Length} was expected.");

                var data = new byte[1];
                var result = (I2cDriver.ReadNative(hndle, data, 1)).ToInt32();
                if (result != 1)
                    throw new InvalidOperationException($"Read {result} instead of {1}");

                return data;
            }
        }

        public void TurnOff()
        {
            TurnOff(Enumerable.Range(0, RelayCount - 1).Select(i => (ushort)i));
        }

        public void TurnOn()
        {
            TurnOn(Enumerable.Range(0, RelayCount - 1).Select(i => (ushort)i));
        }

        object IRelayBoardDriver.ReloadState()
        {
            throw new NotImplementedException();
        }

        public async Task Init(Context context)
        {
            if (!_isInitialized)
            {
                SetOutputMode();
                InitializeRelayState();
                _isInitialized = true;
            }
        }
    }
}