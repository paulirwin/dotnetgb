using System;

namespace DotNetGB.Hardware
{
    public interface IController
    {
        event EventHandler<Button> OnButtonPress;

        event EventHandler<Button> OnButtonRelease;
    }
}
