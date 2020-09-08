using System;
using System.IO;

namespace DotNetGB.Hardware.Cartridges.Battery
{
    public class FileBattery : IBattery
    {
        private readonly FileInfo _saveFile;

        public FileBattery(FileSystemInfo parent, string baseName)
        {
            _saveFile = new FileInfo(Path.Join(parent.FullName, baseName + ".sav"));
        }

        public void LoadRam(int[] ram) => LoadRamWithClock(ram, null);

        public void SaveRam(int[] ram) => SaveRamWithClock(ram, null);

        public void LoadRamWithClock(int[] ram, long[]? clockData)
        {
            if (!_saveFile.Exists)
            {
                return;
            }

            long saveLength = _saveFile.Length;
            saveLength = saveLength - (saveLength % 0x2000);
            using (var inputStream = _saveFile.OpenRead())
            {
                LoadRam(ram, inputStream, saveLength);
                if (clockData != null)
                {
                    LoadClock(clockData, inputStream);
                }
            }
        }

        public void SaveRamWithClock(int[] ram, long[]? clockData)
        {
            using (var os = _saveFile.OpenWrite())
            {
                SaveRam(ram, os);
                if (clockData != null)
                {
                    SaveClock(clockData, os);
                }
            }
        }

        private void LoadClock(long[] clockData, Stream inputStream)
        {
            using (var binReader = new BinaryReader(inputStream))
            {
                int i = 0;
                while (binReader.BaseStream.Position != binReader.BaseStream.Length)
                {
                    clockData[i++] = binReader.ReadInt32() & 0xffffffff;
                }
            }
        }

        private void SaveClock(long[] clockData, Stream os)
        {
            using (var binWriter = new BinaryWriter(os))
            {
                foreach (long d in clockData)
                {
                    binWriter.Write((int) d);
                }
            }
        }

        private void LoadRam(int[] ram, Stream inputStream, long length)
        {
            byte[] buffer = new byte[ram.Length];
            inputStream.Read(buffer, 0, Math.Min((int) length, ram.Length));
            for (int i = 0; i < ram.Length; i++)
            {
                ram[i] = buffer[i] & 0xff;
            }
        }

        private void SaveRam(int[] ram, Stream os)
        {
            byte[] buffer = new byte[ram.Length];
            for (int i = 0; i < ram.Length; i++)
            {
                buffer[i] = (byte) (ram[i]);
            }

            os.Write(buffer);
        }
    }
}