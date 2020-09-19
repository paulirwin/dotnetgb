namespace DotNetGB.Hardware
{
    public interface IDisplay
    {
        void PutDmgPixel(int color);

        void PutColorPixel(int gbcRgb);

        void RequestRefresh();

        void WaitForRefresh();

        void EnableLcd();

        void DisableLcd();

        void Run();
        
        void Stop();
    }
}