using System;
using CsRex;

namespace CsRex {
  internal readonly struct Instruction {
    internal readonly byte Id;
    internal readonly byte Length;
    internal readonly ushort Parameter;

    internal Instruction (byte id, byte length = 0, ushort parameter = 0) {
      Id = id;
      Length = length;
      Parameter = parameter;
    }
  }
}
