﻿namespace DotNetGB.Hardware.Cartridges.Battery
{
    public interface IBattery
    {
        void LoadRam(int[] ram);

        void SaveRam(int[] ram);

        void LoadRamWithClock(int[] ram, long[] clockData);

        void SaveRamWithClock(int[] ram, long[] clockData);
    }
}
