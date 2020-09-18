namespace DotNetGB.Hardware
{
    public interface ISerialEndpoint
    {
        int Transfer(int outgoing);
    }
}
