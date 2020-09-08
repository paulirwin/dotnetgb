namespace DotNetGB.Hardware
{
    public interface IAddressSpace
    {
        bool Accepts(int address);

        int this[int address] { get; set; }
    }
}