using System;
using CsRex;

namespace CsRex {
  internal readonly struct Instruction {
    internal readonly Opcode Op;
    internal readonly byte Length;
    internal readonly ushort Parameter;

    internal Instruction (Opcode op, byte length = 0, ushort parameter = 0) {
      Op = op;
      Length = length;
      Parameter = parameter;
    }
  }
}
