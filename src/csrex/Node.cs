using System;
using System.Collections.Generic;
using CsRex;

namespace CsRex {
  internal abstract class Node {
    internal virtual Node Add (Node n) {
      List<Node> list;

      list = new List<Node>();
      list.Add(this);
      list.Add(n);
      return new GroupNode(list);
    }

    internal abstract List<ushort> GetBytecode ();
  }
}
