using System;

namespace DotNetGB.Hardware
{
    public class SerialPort : IAddressSpace
    {
        private readonly ISerialEndpoint serialEndpoint;

        private readonly InterruptManager interruptManager;

        private readonly SpeedMode speedMode;

        private int sb;

        private int sc;

        private bool transferInProgress;

        private int divider;

        public SerialPort(InterruptManager interruptManager, ISerialEndpoint serialEndpoint, SpeedMode speedMode)
        {
            this.interruptManager = interruptManager;
            this.serialEndpoint = serialEndpoint;
            this.speedMode = speedMode;
        }

        public void Tick()
        {
            if (!transferInProgress)
            {
                return;
            }
            if (++divider >= Gameboy.TICKS_PER_SEC / 8192 / speedMode.Mode)
            {
                transferInProgress = false;
                
                try
                {
                    sb = serialEndpoint.Transfer(sb);
                }
                catch
                {
                    //LOG.error("Can't transfer byte", e);
                    sb = 0;
                }

                interruptManager.RequestInterrupt(InterruptManager.InterruptType.Serial);
            }
        }
        
        public bool Accepts(int address) => address == 0xff01 || address == 0xff02;

        public int this[int address]
        {
            get
            {
                return address switch
                {
                    0xff01 => sb,
                    0xff02 => sc | 0b01111110,
                    _ => throw new ArgumentException()
                };
            }
            set
            {
                if (address == 0xff01)
                {
                    sb = value;
                }
                else if (address == 0xff02)
                {
                    sc = value;
                    if ((sc & (1 << 7)) != 0)
                    {
                        StartTransfer();
                    }
                }
            }
        }

        private void StartTransfer()
        {
            transferInProgress = true;
            divider = 0;
        }
    }
}
