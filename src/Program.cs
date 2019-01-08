using System;
using CsRex;

namespace CsRex {
  public class Program {
    static int Main (string[] args) {
      Regex r;
      Match m;

      while (true) {
        Console.Write("regex: ");
        r = new Regex(Console.ReadLine());
        Console.Write("\n");

        r.Dump();

        Console.Write("text: ");
        r.Match(Console.ReadLine(), out m);
        Console.Write("\n");
        Console.Write(" success: {0}, index: {1}, length: {2}\n\n", m.Success, m.Index, m.Length);
      }
    }
  }
}
