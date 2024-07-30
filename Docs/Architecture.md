# Architecture

> Some/most of this may refer to code that does not yet exist, it's mostly a way for me to "plan" out future work.

This is a high level architecture description of the Solis compiler.

The solis compiler follows compiler standards pretty closely so there isn't anything here that is too unusual.

## Pipeline

The compiler is a series of pipelined steps that transform the previous stage's output into a new format.  Sometimes they just mutate the structure in-place.

### Overall Structure

1. Lexing, this splits each file into a series of tokens.
  - This does numeric parsing and symbol grouping (math operators vs logical operators).
  - Symbol grouping is mostly arbitrary and may be removed in future for just the absolute token
2. Parsing, this groups tokens into AST structures
  - Handles precedence through some algorithm (that I've since forgotten the name of) is similar to a Pratt style/Precedence table
  - This also performs early typechecking by doing type unification (Hindley-Milner type system-like)
  - This also builds the SolisDefinitionTable
3. Transpilation/Compilation
  - For simplicity we just refer to this as transpilation since we don't directly target AOT atm (we go through C#)

We use a fan in/out approach where we spread out the work of lexing/parsing across many cores (each file can be independently compiled).  We do have to barrier wait for all parsing to complete before we can execute transiplation since we need fresh types to be resolved (which could be across file boundary).

Currently we just support one translation unit per compilation.

> If you want to compile multiple translation units they need to be independent but they can be scheduled through just a standard `Parallel.ForEach` (or similar).  It's probably recommended that if you are aiming to compile many translation units (i.e. mods) you should set the compiler to use just one core for each else the context switching might result in some overall slower progress.

# Caching

We don't do any major caching right now but we support a few ways to improve performance;
1. Pre-building assemblies, you can pre-build our internal data structures (definitions) for your referenced code/assemblies so that when you compile Solis against it they don't need to do heavy reflection / analysis to build external definitions.  I don't recommend doing this during development but as a sort of "release" build it helps.  This mainly improves multiple translation unit compilation since otherwise they all have to repeat the same analysis of external referenced assemblies.
2. 

## Future Work

- Caching IL / files (checksum)

# Hot Reloading

We support hot reloading when running the code in hot-interpreter mode.  It's highly recommended to use this because;
1. It has lower compilation times
2. It has internal caches for assemblies (lowering compile times further)
3. Any change can be hot-reloaded

The last point is very important since if you look at the list of [supported c# edits](https://github.com/dotnet/roslyn/blob/main/docs/wiki/EnC-Supported-Edits.md) you'll notice that it's a pretty small list and a ton of common operations aren't supported.

We support *effectively* any change for hot-reload because when running in interpreter mode we don't actually generate any C# structures that are built ontop of definitions.

The only exception is that if you change significantly a method that you are currently executing it might not be able to find a valid position to continue from or might find itself in an invalid state.

For example;

```cs
func sum(): int {
    let a = 1;
    console.write("Foo");
// -> we are here (next line)
    console.write("Bar");
    return a + 2;
}
```

Modifying this to be

```cs
func sum(a: int, b: int): int {
// -> we are here
    return a + b;
}
```

The issues being:
1. The line you were on have been removed and line you just executed was removed
2. New parameters that haven't been passed in
3. Removing a local variable in scope

There isn't too much we can do here since if you imagine that the calling function *also* has changed it sort of can become a very hard problem figuring out how far up we would have to "rerun" and you run into issues of impure functions.

So you can either "exit" or "force continue" where the code could potentially just crash.  In this case it would set b = 0 and a would remain (since it's defined in scope) so the result would be 1 + 0 = 1.

We do support "rewinding" (or historical debugging) so you could also just rewind it up one call then re-enter the function yourself to get the intended result, we don't rewind any mutations though so if you have a shared list (or the console.write for example) those will still stay "executed".
