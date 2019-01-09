using System;
using CsRex.Parsing;

namespace CsRex.Parsing {
  public class ParsingException : Exception {
    public ParsingException (string message) : base(message) {}
  }
}
