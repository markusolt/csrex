using System;
using CsRex;

namespace CsRex {
  public readonly struct Match {
    public readonly bool Success;
    public readonly int Index;
    public readonly int Length;

    internal Match (bool success, int index = 0, int length = 0) {
      Success = success;
      Index = index;
      Length = length;
    }
  }
}
