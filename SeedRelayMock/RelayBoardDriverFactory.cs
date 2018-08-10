using RelayWebApi.Drivers;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedRelayMock
{
    public static class RelayBoardDriverFactory
    {
        public static IRelayBoardDriver ForFixedRelay(params bool?[] initialState)
        {
            var inst = new MockRelayBoardDriver
            {
                RelayCount = (ushort)initialState.Length,
                WorkingMode = MockWorkingMode.Limited,
                StateSelector = (index) => initialState[index]
            }.Initialize();
            return inst;
        }
        public static IRelayBoardDriver ForUnlimitedRelay(bool? defaultValue)
        {
            var inst = new MockRelayBoardDriver
            {
                WorkingMode = MockWorkingMode.Unlimited,
                StateSelector = (index) => defaultValue
            }.Initialize();
            return inst;
        }
    }
}
