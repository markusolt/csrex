# csrex

A simple implementation of regular expressions in C#.

Regular expressions are first compiled into an array of instructions.
The resulting regex object can then attempt to match a given string without needing to perform any memory allocations.
Instead of a string the `match()` method also accepts the new `ReadOnlySpan<char>` type.
