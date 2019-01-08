using System;
using CsRex;

namespace CsRex {
  public class ParsingException : Exception {
    public ParsingException (string message) : base(message) {}
  }
}
