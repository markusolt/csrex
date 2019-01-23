using System;
using CsRex;

namespace CsRex {
  internal class ThreadManager {
    private bool[] _visited;
    private Thread[] _threads; // length must be a power of 2
    private int _delimiter;
    private int _read;
    private int _write;
    private int _skipSleep;
    private int _minSleep;

    internal ThreadManager (int length) {
      _visited = new bool[length];
      _threads = new Thread[1];
      _delimiter = 0;
      _read = 0;
      _write = 0;
      _skipSleep = 0;
      _minSleep = -1;
    }

    internal void Dump () {
      Console.Write("--------------------------------\nThreads:\n");
      Console.Write(" del: {0,2}: read: {1,2}: write: {2,2}:\n\n", _delimiter, _read, _write);
      for (int i = 0; i < _threads.Length; i++) {
        Console.Write("  {0,2}: {1,2}\n", i, _threads[i].Position);
      }
      Console.Write("--------------------------------\n\n");
    }

    internal void Clear () {
      Array.Clear(_visited, 0, _visited.Length);
      _read = 0;
      _write = 0;
    }

    internal bool Swap (out int skip) {
      if (_write == 0) {
        skip = 0;
        return false;
      }

      Array.Clear(_visited, 0, _visited.Length);
      _delimiter = (_delimiter + _write) & (_threads.Length - 1);
      _read = _write;
      _write = 0;
      _skipSleep = _minSleep;
      _minSleep = -1;
      skip = _skipSleep; // 0 means no skip
      return true;
    }

    internal bool TryPull (out int t) {
      int pos;

      pos = (_delimiter - _read) & (_threads.Length - 1);
      while (_threads[pos].Sleep > _skipSleep || _read > 0 && _visited[_threads[pos].Position]) {
        if (_threads[pos].Sleep > _skipSleep) {
          Push(_threads[pos].Position, _threads[pos].Sleep - _skipSleep - 1);
        }

        _read--;
        pos = (_delimiter - _read) & (_threads.Length - 1);
      }

      if (_read == 0) {
        t = -1;
        return false;
      }

      t = _threads[pos].Position;
      _visited[t] = true;
      _read--;
      return true;
    }

    internal void Push (int t, int sleep = 0) {
      int pos;

      if (_read + _write == _threads.Length) {
        _increaseCapacity();
      }

      if (_minSleep == -1 || _minSleep > sleep) {
        _minSleep = sleep;
      }

      _write++;
      pos = (_delimiter + _write - 1) & (_threads.Length - 1);
      _threads[pos] = new Thread(t, sleep);
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
      _threads[pos] = new Thread(t, _skipSleep);
    }

    private void _increaseCapacity () {
      int pos;
      Thread[] tmp;

      pos = (_delimiter - _read) & (_threads.Length - 1);
      tmp = new Thread[_threads.Length * 2];
      Array.Copy(_threads, pos, tmp, 0, _threads.Length - pos);
      Array.Copy(_threads, 0, tmp, _threads.Length - pos, pos);

      _threads = tmp;
      _delimiter = _read;
    }
  }
}
