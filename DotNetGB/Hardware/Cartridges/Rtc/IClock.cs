namespace DotNetGB.Hardware.Cartridges.Rtc
{
    public interface IClock
    {
        long CurrentTimeMillis { get; }
    }
}
