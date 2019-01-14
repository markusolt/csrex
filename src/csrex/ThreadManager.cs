using System;
using CsRex;

namespace CsRex {
  internal class ThreadManager {
    private bool[] _visited;
    private int[] _threads; // length must be a power of 2
    private int _delimiter;
    private int _read;
    private int _write;

    internal ThreadManager (int length) {
      _visited = new bool[length];
      _threads = new int[1];
      _delimiter = 0;
      _read = 0;
      _write = 0;
    }

    internal void Dump () {
      Console.Write("--------------------------------\nThreads:\n");
      Console.Write(" del: {0,2}: read: {1,2}: write: {2,2}:\n\n", _delimiter, _read, _write);
      for (int i = 0; i < _threads.Length; i++) {
        Console.Write("  {0,2}: {1,2}\n", i, _threads[i]);
      }
      Console.Write("--------------------------------\n\n");
    }

    internal void Clear () {
      Array.Clear(_visited, 0, _visited.Length);
      _read = 0;
      _write = 0;
    }

    internal bool Swap () {
      if (_write == 0) {
        return false;
      }

      Array.Clear(_visited, 0, _visited.Length);
      _delimiter = (_delimiter + _write) & (_threads.Length - 1);
      _read = _write;
      _write = 0;
      return true;
    }

    internal bool TryPull (out int t) {
      int pos;

      pos = (_delimiter - _read) & (_threads.Length - 1);
      while (_read > 0 && _visited[_threads[pos]]) {
        _read--;
        pos = (_delimiter - _read) & (_threads.Length - 1);
      }

      if (_read == 0) {
        t = -1;
        return false;
      }

      t = _threads[pos];
      _visited[t] = true;
      _read--;
      return true;
    }

    internal void Push (int t) {
      int pos;

      if (_read + _write == _threads.Length) {
        _increaseCapacity();
      }

      _write++;
      pos = (_delimiter + _write - 1) & (_threads.Length - 1);
      _threads[pos] = t;
    }

    internal void PushFront (int t) {
      int pos;

      if (_visited[t]) {
        return;
      }

      if (_read + _write == _threads.Length) {
        _increaseCapacity();
      }

      _read++;
      pos = (_delimiter - _read) & (_threads.Length - 1);
      _threads[pos] = t;
    }

    private void _increaseCapacity () {
      int pos;
      int[] tmp;

      pos = (_delimiter - _read) & (_threads.Length - 1);
      tmp = new int[_threads.Length * 2];
      Array.Copy(_threads, pos, tmp, 0, _threads.Length - pos);
      Array.Copy(_threads, 0, tmp, _threads.Length - pos, pos);

      _threads = tmp;
      _delimiter = _read;
    }
  }
}
