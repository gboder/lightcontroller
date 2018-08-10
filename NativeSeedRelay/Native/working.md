        public byte[] MyReadBytes(byte registerAddress, int startAddress, int size)
        {
            var hndle = GetI2cBusHandle();
            if (Ioctl(hndle, 0x0703, 0x20) < 0)
                throw new InvalidOperationException($"Unable to control device ");

            // Write address
            var wData = new byte[]{registerAddress};
            var wresult = Write(hndle,wData,wData.Length).ToInt32();
            if(wresult != wData.Length)
                throw new InvalidOperationException($"Unable to write {registerAddress.ToString("X")} to device.");

            var data = new byte[size];
            var result = (Read(hndle,data,size)).ToInt32();
            if(result != size)
                throw new InvalidOperationException($"Read {result} instea of {size}");    
            
            return data;
        }