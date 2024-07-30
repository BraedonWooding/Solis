# Solis IL

We have a pretty simple compilation process (with the intention of fast realtime compilation).

We have a reasonably high level IL called SIL (Solis IL) then a lower level bytecode styled IL called BIL (bytecode IL).

The bytecode IL is meant to be directly executable (though technically even SIL can be interpreted) and is a simple Lua like IL that is built to be fast to execute.  This is primarily for usecases like C++ where:
- Transpilation is expensive / annoying (having to ship a C++ compiler & cost of compiling C++)
- Compilation is difficult (direct to x86) and is not a first MVP priority.

What makes our IL different is that it exposes macro like functions that a "target" can implement to allow multiple host languages for transpilation but also to avoid wrapper collections.


