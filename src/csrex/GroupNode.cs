using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal class GroupNode : Node {
    private List<Node> _children;

    internal GroupNode () {
      _children = new List<Node>();
    }

    internal GroupNode (List<Node> children) {
      _children = children;
    }

    internal override Node Add (Node n) {
      _children.Add(n);
      return this;
    }

    internal override List<ushort> GetBytecode () {
      List<ushort> p;

      p = new List<ushort>();
      foreach (Node c in _children) {
        p.AddRange(c.GetBytecode());
      }
      return p;
    }
  }
}
