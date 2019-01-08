using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal class CharacterNode : Node {
    private char _c;

    internal CharacterNode (char c) {
      _c = c;
    }

    internal override List<ushort> GetBytecode () {
      List<ushort> p;

      p = new List<ushort>();
      p.AddRange(new ushort[] {Regex.instr_char, _c});
      return p;
    }
  }
}
