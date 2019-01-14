using System;
using CsRex;

namespace CsRex {
  internal enum Opcode : byte {
    Character,
    Range,
    Class,
    BranchFast,
    BranchbackFast,
    Branch,
    Branchback,
    Jump,
    Jumpback,
  }
}
