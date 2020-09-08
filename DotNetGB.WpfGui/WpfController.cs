using System;
using System.Windows.Input;
using DotNetGB.Hardware;

namespace DotNetGB.WpfGui
{
    public class WpfController : IController
    {
        public event EventHandler<Button> OnButtonPress;
        public event EventHandler<Button> OnButtonRelease;

        public void ButtonPressed(Key key)
        {
            var button = Translate(key);

            if (button != null)
            {
                OnButtonPress?.Invoke(this, button.Value);
            }
        }

        public void ButtonReleased(Key key)
        {
            var button = Translate(key);

            if (button != null)
            {
                OnButtonRelease?.Invoke(this, button.Value);
            }
        }

        private static Button? Translate(Key key)
        {
            switch (key)
            {
                case Key.Left:
                    return Button.LEFT;
                case Key.Right:
                    return Button.RIGHT;
                case Key.Up:
                    return Button.UP;
                case Key.Down:
                    return Button.DOWN;
                case Key.Z:
                    return Button.A;
                case Key.X:
                    return Button.B;
                case Key.Enter:
                    return Button.START;
                case Key.Back:
                    return Button.SELECT;
            }

            return null;
        }
    }
}