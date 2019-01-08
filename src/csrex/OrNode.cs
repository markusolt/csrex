using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal class OrNode : Node {
    private Node _child1;
    private Node _child2;

    internal OrNode (Node child1, Node child2) {
      _child1 = child1;
      _child2 = child2;
    }

    internal override List<ushort> GetBytecode () {
      List<ushort> p1;
      List<ushort> p2;

      p1 = _child1.GetBytecode();
      p2 = _child2.GetBytecode();

      p1.AddRange(new ushort[] {Regex.instr_jump, (ushort) p2.Count});
      p1.InsertRange(0, new ushort[] {Regex.instr_branch, (ushort) p1.Count});

      p1.AddRange(p2);
      return p1;
    }
  }
}
