namespace DotNetGB.Hardware
{
    public interface ISoundOutput
    {
        void Start();

        void Stop();

        void Play(byte left, byte right);
    }

    public sealed class NullSoundOutput : ISoundOutput
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Play(byte left, byte right)
        {
        }
    }
}
