using Common;
using RelayWebApi.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeedRelayMock
{
    internal enum MockWorkingMode : ushort
    {
        /// <summary>
        /// Will thrown an exception in case a relay outside the defined bound is used
        /// </summary>
        Limited,
        /// <summary>
        /// Will accepts any positive relay index
        /// </summary>
        Unlimited
    }
    internal class MockedRelay
    {
        public bool? State { get; set; }
        public ushort Index { get; set; }

    }

    internal class MockRelayBoardDriver : IRelayBoardDriver
    {
        internal MockWorkingMode WorkingMode { get; set; }
        private readonly List<MockedRelay> mockedRelays = new List<MockedRelay>();
        internal bool? DefaultState { get; set; }

        private MockedRelay GetRelay(ushort index)
        {
            var r = mockedRelays.FirstOrDefault(sr => sr.Index == index);
            if (r != null)
                return r;
            else if (WorkingMode == MockWorkingMode.Unlimited)
            {
                // Create, add and return a new mocked relay
                AddRelay(CreateRelay(index));
                return GetRelay(index);
            }
            // Outside of the known relay index and working in mode limited
            throw new IndexOutOfRangeException($"{index} is outside of the range 0..{mockedRelays.Count} and working mode is {WorkingMode}");
        }

        private void AddRelay(MockedRelay r)
        {
            //if (WorkingMode == MockWorkingMode.Limited && RelayCount >= mockedRelays.Count)
            //    throw new InvalidOperationException($"Unable to add a new mocked relay to the internal state collection. Wokring mode is {WorkingMode}, mocked driver is configured to accept {RelayCount} and internal state contains already {mockedRelays.Count} instances.");

            //if (WorkingMode == MockWorkingMode.Limited && r.Index >= mockedRelays.Count)
            //    throw new InvalidOperationException($"Index {r.Index} is greather than the number of accepted relay {RelayCount}");

            if (mockedRelays.Any(sr => sr.Index == r.Index))
            {
                mockedRelays.Remove(mockedRelays.FirstOrDefault(sr => sr.Index == r.Index));
                //throw new InvalidOperationException($"Index {r.Index} already exists into the internal mocked relay store.");
            }
            mockedRelays.Add(r);
        }

        private MockedRelay CreateRelay(ushort index)
        {
            return new MockedRelay { Index = index, State = DefaultState };
        }

        private int AdaptRelayState(MockedRelay r)
        {
            /*
             * Takes
             *  1) Relay's state
             *      when null uses DefaultState
             *      when null defaults to false
             *  2) When false returns 0
             *      when true returns 1
             */
            return r?.State ?? DefaultState ?? false ? 1 : 0;
        }
        private void InitializeMockStore(ushort relayCount) => Enumerable
                .Range(0, relayCount)
                .Select(i => new MockedRelay
                {
                    Index = (ushort)i,
                    State = StateSelector((ushort)i)
                })
                .Iter(AddRelay);
        private void SetState(MockedRelay mockedRelay, bool? state)
        {
            mockedRelay.State = state;
        }

        private ushort _relayCount;
        public ushort RelayCount
        {
            get
            {
                return WorkingMode == MockWorkingMode.Limited ? _relayCount : (ushort)mockedRelays.Count;
            }
            set
            {
                if (WorkingMode == MockWorkingMode.Limited)
                {
                    _relayCount = value;
                    Initialize();
                }
            }
        }
        internal Func<ushort, bool?> StateSelector { get; set; }
        public bool IsInitialized { get; private set; }

        public int GetState(ushort relay)
        {
            return AdaptRelayState(GetRelay(relay));
        }

        public IEnumerable<int> GetState()
        {
            return mockedRelays.OrderBy(r => r.Index).Select(AdaptRelayState);
        }

        public IRelayBoardDriver Initialize()
        {
            if (StateSelector == null)
                StateSelector = idx => DefaultState;

            if (RelayCount > 0)
                InitializeMockStore(RelayCount);

            return this;
        }

        public byte[] MyReadBytes(byte registerAddress, int startAddress, int size)
        {
            throw new NotImplementedException();
        }

        public object ReloadState()
        {
            throw new NotImplementedException();
        }

        public void TurnOff() => mockedRelays.Iter(r => TurnOff(r.Index));

        public void TurnOff(IEnumerable<ushort> relays) => relays.Iter(TurnOff);

        public void TurnOff(ushort relay) => SetState(GetRelay(relay), false);

        public void TurnOn() => mockedRelays.Iter(r => TurnOn(r.Index));

        public void TurnOn(ushort relay) => SetState(GetRelay(relay), true);

        public void TurnOn(IEnumerable<ushort> relays) => relays.Iter(TurnOn);

        public async Task Init(Context context)
        {
            if (!IsInitialized)
            {
                // Init;
                Initialize();
                IsInitialized = true;
            }
        }
    }
}
