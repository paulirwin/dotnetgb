using System;
using System.Windows.Input;
using DotNetGB.Hardware;

namespace DotNetGB.WpfGui
{
    public class WpfController : IController
    {
        public event EventHandler<Button>? OnButtonPress;
        public event EventHandler<Button>? OnButtonRelease;

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
            return key switch
            {
                Key.Left => Button.LEFT,
                Key.Right => Button.RIGHT,
                Key.Up => Button.UP,
                Key.Down => Button.DOWN,
                Key.Z => Button.A,
                Key.X => Button.B,
                Key.Enter => Button.START,
                Key.Back => Button.SELECT,
                _ => null
            };
        }
    }
}