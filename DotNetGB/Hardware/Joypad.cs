using System.Collections.Generic;

namespace DotNetGB.Hardware
{
    public class Joypad : IAddressSpace
    {
        private readonly ISet<Button> _buttons = new HashSet<Button>();

        private int _p1;

        public Joypad(InterruptManager interruptManager, IController controller)
        {
            controller.OnButtonPress += (sender, button) =>
            {
                interruptManager.RequestInterrupt(InterruptManager.InterruptType.P10_13);
                _buttons.Add(button);
            };

            controller.OnButtonRelease += (sender, button) => _buttons.Remove(button);
        }

        public bool Accepts(int address) => address == 0xff00;

        public int this[int address]
        {
            get
            {
                int result = _p1 | 0b11001111;
                foreach (var b in _buttons)
                {
                    if ((b.Line & _p1) == 0)
                    {
                        result &= 0xff & ~b.Mask;
                    }
                }
                return result;
            }
            set => _p1 = value & 0b00110000;
        }
    }
}
