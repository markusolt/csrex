using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal class RepeatNode : Node {
    private Node _child;

    internal RepeatNode (Node child) {
      _child = child;
    }

    internal override List<ushort> GetBytecode () {
      List<ushort> p;

      p = _child.GetBytecode();
      p.AddRange(new ushort[] {Regex.instr_branchback, (ushort) p.Count});
      return p;
    }
  }
}
