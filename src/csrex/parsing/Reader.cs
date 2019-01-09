using System;
using System.Collections.Generic;
using System.IO;
using CsRex.Parsing;

namespace CsRex.Parsing {
  internal class Reader {
    private string _text;
    private int _pos;

    private const char _eof = '\0';

    internal Reader (string text) {
      if (text == null) {
        throw new ArgumentNullException(nameof(text));
      }

      _text = text;
      _pos = 0;
    }

    internal bool EndOfFile {
      get {
        return _pos >= _text.Length;
      }
    }

    internal char Peek (int offset = 0) {
      if (_pos + offset >= _text.Length) {
        return _eof;
      }

      return _text[_pos + offset];
    }

    internal char Read () {
      if (_pos >= _text.Length) {
        return _eof;
      }

      return _text[_pos++];
    }
  }
}
