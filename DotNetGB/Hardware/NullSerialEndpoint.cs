namespace DotNetGB.Hardware
{
    public class NullSerialEndpoint : ISerialEndpoint
    {
        public int Transfer(int outgoing) => 0;
    }
}
