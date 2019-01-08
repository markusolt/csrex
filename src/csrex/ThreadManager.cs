using System;
using CsRex;

namespace CsRex {
  internal class ThreadManager {
    private bool[] _visited;
    private int[] _iterator;
    private int _iteratorLength;
    private int _it;

    internal ThreadManager (int length) {
      _visited = new bool[length];
      _iterator = new int[1];
      _iteratorLength = 0;
      _it = 0;
    }

    internal int Count {
      get {
        return _iteratorLength;
      }
    }

    internal int Current {
      get {
        return _iterator[_it];
      }
    }

    internal void Clear () {
      _iteratorLength = 0;
    }

    internal void Reset () {
      for (int i = 0; i < _visited.Length; i++) {
        _visited[i] = false;
      }
      _it = 0;
    }

    internal bool TryPull (out int t) {
      while (_it < _iteratorLength && _visited[_iterator[_it]]) {
        _iterator[_it] = _iterator[--_iteratorLength];
      }

      if (_it >= _iteratorLength) {
        t = -1;
        return false;
      }

      t = _iterator[_it];
      _iterator[_it] = _iterator[--_iteratorLength];
      _visited[t] = true;
      return true;
    }

    internal void PushBack (int t) {
      if (_iterator.Length == _iteratorLength) {
        Array.Resize(ref _iterator, _iterator.Length * 2);
      }
      _iterator[_iteratorLength++] = _iterator[_it];
      _iterator[_it] = t;
      _it++;
    }

    internal void Push (int t) {
      if (_visited[t]) {
        return;
      }

      if (_iterator.Length == _iteratorLength) {
        Array.Resize(ref _iterator, _iterator.Length * 2);
      }
      _iterator[_iteratorLength++] = t;
    }
  }
}
