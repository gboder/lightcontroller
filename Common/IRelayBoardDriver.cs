using Common;
using System.Collections.Generic;

namespace RelayWebApi.Drivers
{
    public interface IRelayBoardDriver : IInitializable
    {
        void TurnOff();
        void TurnOff(IEnumerable<ushort> relays);
        void TurnOff(ushort relay);
        void TurnOn();
        void TurnOn(ushort relay);
        void TurnOn(IEnumerable<ushort> relays);
        int GetState(ushort relay);
        IEnumerable<int> GetState();
        byte[] MyReadBytes(byte registerAddress, int startAddress, int size);

        ushort RelayCount { get; }

        object ReloadState();
    }
}