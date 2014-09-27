using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer
{
    internal class GraphicState
    {
    }

    internal abstract class Instruction
    {
        public abstract bool Matches(byte opcode);
        public virtual int EatInstructionStream(byte[] instructions, int ip, Stack<UInt32> stack) { return ip; }
        public abstract void Execute(GraphicState state, Stack<UInt32> stack, params bool[] flags);
    }

    internal class NOP : Instruction
    {
        public override bool Matches(byte opcode) { return true; } // take the rest
        public override void Execute(GraphicState state, Stack<UInt32> stack, params bool[] flags)
        {
        }
    }
    
    internal class NPushB : Instruction
    {
        public override bool Matches(byte opcode) { return opcode == 0x40; }
        public override int EatInstructionStream(byte[] instructions, int ip, Stack<UInt32> stack)
        {
            byte n = instructions[++ip];
            for (int i = 0; i < n; ++i)
            {
                stack.Push(instructions[++ip]); // TODO: instructions may overrun?
            }
            return ip;
        }
        public override void Execute(GraphicState state, Stack<UInt32> stack, params bool[] flags)
        {
        }
    }

    internal class MDAP : Instruction
    {
        public override bool Matches(byte opcode) { return (opcode & 0xfe) == 0x2e; }
        public override void Execute(GraphicState state, Stack<UInt32> stack, params bool[] flags)
        {
        }
    }

    internal class Interpreter
    {
        private readonly Instruction[] _lookup;

        public Interpreter()
        {
            var instructions = new List<Instruction>
            {
                new NPushB(),
                new MDAP(),
                new NOP()
            };
            
            _lookup = BuildLookup(instructions);
        }

        private Instruction[] BuildLookup(List<Instruction> instructions)
        {
            var result = new Instruction[256];
            for (int opcode = 0; opcode < 256; opcode++)
            {
                result[opcode] = instructions.First(i => i.Matches((byte)opcode));
            }
            return result;
        }

        public void Run(byte[] instructions)
        {
            var stack = new Stack<UInt32>();
            var state = new GraphicState();
            for (int ip = 0; ip < instructions.Length; ip++)
            {
                var opcode = instructions[ip];
                var instruction = _lookup[opcode];
                ip = instruction.EatInstructionStream(instructions, ip, stack);
                instruction.Execute(state, stack);
            }
        }
    }
}
