using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal class OptionalNode : Node {
    private Node _child;

    internal OptionalNode (Node child) {
      _child = child;
    }

    internal override List<ushort> GetBytecode () {
      List<ushort> p;

      p = _child.GetBytecode();
      p.InsertRange(0, new ushort[] {Regex.instr_branch, (ushort) p.Count});
      return p;
    }
  }
}
