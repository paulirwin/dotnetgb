namespace DotNetGB.Hardware
{
    public interface ISoundOutput
    {
        void Start();

        void Stop();

        void Play(int left, int right);
    }

    public sealed class NullSoundOutput : ISoundOutput
    {
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Play(int left, int right)
        {
        }
    }
}
