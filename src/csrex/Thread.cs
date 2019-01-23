using System;
using CsRex;

namespace CsRex {
  internal struct Thread {
    internal int Position;
    internal int Sleep;

    internal Thread (int position, int sleep = 0) {
      Position = position;
      Sleep = sleep;
    }
  }
}
