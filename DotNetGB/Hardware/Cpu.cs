using System;
using System.Collections.Generic;
using System.Linq;
using DotNetGB.Hardware.CpuOpcodes;
using DotNetGB.Util;

namespace DotNetGB.Hardware
{
    public class Cpu : ITickable
    {
        public enum CpuState
        {
            OPCODE,
            EXT_OPCODE,
            OPERAND,
            RUNNING,
            IRQ_READ_IF,
            IRQ_READ_IE,
            IRQ_PUSH_1,
            IRQ_PUSH_2,
            IRQ_JUMP,
            STOPPED,
            HALTED
        }

        private readonly IAddressSpace _addressSpace;

        private readonly InterruptManager _interruptManager;

        private readonly Gpu _gpu;

        private readonly IDisplay _display;

        private readonly SpeedMode _speedMode;

        private int _opcode1, _opcode2;

        private readonly int[] _operand = new int[2];

        private Opcode? _currentOpcode;

        private IReadOnlyList<IOp>? _ops;

        private int _operandIndex;

        private int _opIndex;

        private CpuState _state = CpuState.OPCODE;

        private int _opContext;

        private int _interruptFlag;

        private int _interruptEnabled;

        private InterruptManager.InterruptType? _requestedIrq;

        private int _clockCycle;

        private bool _haltBugMode;

        public Cpu(IAddressSpace addressSpace, InterruptManager interruptManager, Gpu gpu, IDisplay display, SpeedMode speedMode)
        {
            Registers = new Registers();
            _addressSpace = addressSpace;
            _interruptManager = interruptManager;
            _gpu = gpu;
            _display = display;
            _speedMode = speedMode;
        }

        public void Tick()
        {
            if (++_clockCycle >= (4 / _speedMode.Mode))
            {
                _clockCycle = 0;
            }
            else
            {
                return;
            }

            if (_state == CpuState.OPCODE || _state == CpuState.HALTED || _state == CpuState.STOPPED)
            {
                if (_interruptManager.IsIme && _interruptManager.IsInterruptRequested)
                {
                    if (_state == CpuState.STOPPED)
                    {
                        _display.EnableLcd();
                    }

                    _state = CpuState.IRQ_READ_IF;
                }
            }

            if (_state == CpuState.IRQ_READ_IF || _state == CpuState.IRQ_READ_IE || _state == CpuState.IRQ_PUSH_1 || _state == CpuState.IRQ_PUSH_2 || _state == CpuState.IRQ_JUMP)
            {
                HandleInterrupt();
                return;
            }

            if (_state == CpuState.HALTED && _interruptManager.IsInterruptRequested)
            {
                _state = CpuState.OPCODE;
            }

            if (_state == CpuState.HALTED || _state == CpuState.STOPPED)
            {
                return;
            }

            bool accessedMemory = false;
            while (true)
            {
                int pc = Registers.PC;
                switch (_state)
                {
                    case CpuState.OPCODE:
                        ClearState();
                        _opcode1 = _addressSpace[pc];
                        accessedMemory = true;
                        if (_opcode1 == 0xcb)
                        {
                            _state = CpuState.EXT_OPCODE;
                        }
                        else if (_opcode1 == 0x10)
                        {
                            _currentOpcode = Opcodes.COMMANDS[_opcode1];
                            _state = CpuState.EXT_OPCODE;
                        }
                        else
                        {
                            _state = CpuState.OPERAND;
                            _currentOpcode = Opcodes.COMMANDS[_opcode1];
                            if (_currentOpcode == null)
                            {
                                throw new InvalidOperationException($"No command for 0x{_opcode1:x2}");
                            }
                        }

                        if (!_haltBugMode)
                        {
                            Registers.IncrementPC();
                        }
                        else
                        {
                            _haltBugMode = false;
                        }

                        break;

                    case CpuState.EXT_OPCODE:
                        if (accessedMemory)
                        {
                            return;
                        }

                        accessedMemory = true;
                        _opcode2 = _addressSpace[pc];
                        if (_currentOpcode == null)
                        {
                            _currentOpcode = Opcodes.EXT_COMMANDS[_opcode2];
                        }

                        if (_currentOpcode == null)
                        {
                            throw new InvalidOperationException($"No command for %0xcb 0x{_opcode2:x2}");
                        }

                        _state = CpuState.OPERAND;
                        Registers.IncrementPC();
                        break;

                    case CpuState.OPERAND:
                        if (_currentOpcode == null)
                        {
                            throw new InvalidOperationException("Operand with null opcode");
                        }

                        while (_operandIndex < _currentOpcode.OperandLength)
                        {
                            if (accessedMemory)
                            {
                                return;
                            }

                            accessedMemory = true;
                            _operand[_operandIndex++] = _addressSpace[pc];
                            Registers.IncrementPC();
                        }

                        _ops = _currentOpcode.Ops;
                        _state = CpuState.RUNNING;
                        break;

                    case CpuState.RUNNING:
                        if (_opcode1 == 0x10)
                        {
                            if (_speedMode.OnStop())
                            {
                                _state = CpuState.OPCODE;
                            }
                            else
                            {
                                _state = CpuState.STOPPED;
                                _display.DisableLcd();
                            }

                            return;
                        }
                        else if (_opcode1 == 0x76)
                        {
                            if (_interruptManager.IsHaltBug)
                            {
                                _state = CpuState.OPCODE;
                                _haltBugMode = true;
                                return;
                            }
                            else
                            {
                                _state = CpuState.HALTED;
                                return;
                            }
                        }

                        if (_opIndex < _ops.Count)
                        {
                            var op = _ops[_opIndex];
                            bool opAccessesMemory = op.ReadsMemory || op.WritesMemory;
                            if (accessedMemory && opAccessesMemory)
                            {
                                return;
                            }

                            _opIndex++;

                            SpriteBug.CorruptionType? corruptionType = op.CausesOemBug(Registers, _opContext);
                            if (corruptionType != null)
                            {
                                HandleSpriteBug(corruptionType.Value);
                            }

                            _opContext = op.Execute(Registers, _addressSpace, _operand, _opContext);
                            op.SwitchInterrupts(_interruptManager);

                            if (!op.Proceed(Registers))
                            {
                                _opIndex = _ops.Count;
                                break;
                            }

                            if (op.ForceFinishCycle)
                            {
                                return;
                            }

                            if (opAccessesMemory)
                            {
                                accessedMemory = true;
                            }
                        }

                        if (_opIndex >= _ops.Count)
                        {
                            _state = CpuState.OPCODE;
                            _operandIndex = 0;
                            _interruptManager.OnInstructionFinished();
                            return;
                        }

                        break;

                    case CpuState.HALTED:
                    case CpuState.STOPPED:
                        return;
                }
            }
        }

        private void HandleInterrupt()
        {
            switch (_state)
            {
                case CpuState.IRQ_READ_IF:
                    _interruptFlag = _addressSpace[0xff0f];
                    _state = CpuState.IRQ_READ_IE;
                    break;

                case CpuState.IRQ_READ_IE:
                    _interruptEnabled = _addressSpace[0xffff];
                    _requestedIrq = null;
                    foreach (var irq in Enum.GetValues(typeof(InterruptManager.InterruptType)).Cast<InterruptManager.InterruptType>())
                    {
                        if ((_interruptFlag & _interruptEnabled & (1 << irq.Ordinal())) != 0)
                        {
                            _requestedIrq = irq;
                            break;
                        }
                    }
                    if (_requestedIrq == null)
                    {
                        _state = CpuState.OPCODE;
                    }
                    else
                    {
                        _state = CpuState.IRQ_PUSH_1;
                        _interruptManager.ClearInterrupt(_requestedIrq.Value);
                        _interruptManager.DisableInterrupts(false);
                    }

                    break;

                case CpuState.IRQ_PUSH_1:
                    Registers.DecrementSP();
                    _addressSpace[Registers.SP] = (Registers.PC & 0xff00) >> 8;
                    _state = CpuState.IRQ_PUSH_2;
                    break;

                case CpuState.IRQ_PUSH_2:
                    Registers.DecrementSP();
                    _addressSpace[Registers.SP] = Registers.PC & 0x00ff;
                    _state = CpuState.IRQ_JUMP;
                    break;

                case CpuState.IRQ_JUMP:
                    Registers.PC = (int)_requestedIrq.GetValueOrDefault();
                    _requestedIrq = null;
                    _state = CpuState.OPCODE;
                    break;
            }
        }

        private void HandleSpriteBug(SpriteBug.CorruptionType type)
        {
            if (!_gpu.Lcdc.IsLcdEnabled)
            {
                return;
            }
            int stat = _addressSpace[GpuRegister.STAT.Address];
            if ((stat & 0b11) == Gpu.GpuMode.OamSearch.Ordinal() && _gpu.TicksInLine < 79)
            {
                SpriteBug.CorruptOam(_addressSpace, type, _gpu.TicksInLine);
            }
        }

        public Registers Registers { get; }

        public void ClearState()
        {
            _opcode1 = 0;
            _opcode2 = 0;
            _currentOpcode = null;
            _ops = null;

            _operand[0] = 0x00;
            _operand[1] = 0x00;
            _operandIndex = 0;

            _opIndex = 0;
            _opContext = 0;

            _interruptFlag = 0;
            _interruptEnabled = 0;
            _requestedIrq = null;
        }

        public CpuState State => _state;

        public Opcode? CurrentOpcode => _currentOpcode;
    }
}