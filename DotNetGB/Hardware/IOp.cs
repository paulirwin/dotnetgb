namespace DotNetGB.Hardware
{
    public interface IOp
    {
        bool ReadsMemory => false;

        bool WritesMemory => false;

        int OperandLength => 0;

        int Execute(Registers registers, IAddressSpace addressSpace, int[] args, int context) => context;

        void SwitchInterrupts(InterruptManager interruptManager) { }

        bool Proceed(Registers registers) => true;

        bool ForceFinishCycle => false;

        SpriteBug.CorruptionType? CausesOemBug(Registers registers, int context) => null;
    }
}
