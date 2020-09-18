using System;

namespace DotNetGB.Hardware
{
    public class NullController : IController
    {
        public event EventHandler<Button> OnButtonPress;
        public event EventHandler<Button> OnButtonRelease;
    }
}
