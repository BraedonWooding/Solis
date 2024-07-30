# Bootstrapping Solis

The first drop for Solis is to have a C# target, but the intention is to eventually look towards other targets like Rust/C++/...

While we could have the compiler in a single language and have bindings for languages this does have a few downsides;
- Annoying requirements; having to ship a GC & language runtime (CLR) is a bit annoying.
  - Solis itself is meant to not be manually memory managed (despite having limited supported) but there are cheaper GC methods such as pooling/arena/ref counting that are in some cases more suitable for games.
- Interpreter Performance, interpreters are much more efficient if written in the base language, if we have to marshal types between an FFI boundary we'll encur pretty significant performance penalties.

So the goal is to have Solis compiled in itself, this way building a target binding can be done through how we define exposed types this would then produce target code which means then the compiler can be run through itself.

I'll likely always keep a C# compiler up to date because we have quite a few C# targets, but I may have the parser to SIL bit at-least be from the binding language.

## Defining a new target

Targets have to be registered in Solis.  This has some sort of weird side-effect that means that every compiler can produce code across all other targets.

```lua
# targets/cpp.sol
const CppTranspiler = ./transpilers/CppTranspiler;
const Compiler = ../Compiler;

// "static" is like main, it's just that it executes on a program level
// this is because we don't support "top" level execution.
fn static () {
    Compiler.RegisterTarget({
        name: "C++",
        // you can still produce .h files but those are referenced as "auxiliary" files
        extension: "cpp",
        // target flag i.e. `solis compile --targets=cpp`
        flag: "cpp",
        // in cpp's case it wants to use SIL to produce it's output not BIL
        // if you are typically compiling to an IL then you want to use BIL (since it's simpler and lower level)
        // but otherwise it's often easier to use SIL since it has easier to map structures
        il_type: .SIL,

        // file to use to perform transpilation
        transpiler: CppTranspiler,
        // each "target" has one transpiler, in C#'s case we actually have a target per one
        // i.e. `--targets=clr,csharp` are our 2 targets.
    });
}
```

Then we can define a transpiler to map our SIL or BIL structure.  The tricky part about a C++ transpiler (compared to a C#/other) is that the definition order matters.  A simple trick is to just forward declare all types.

```lua
const TranspilerTemplate = ./TranspilerTemplate;
const { RecordDef } = ../SIL/Nodes;

const CppTranspiler : TranspilerTemplate = {
    // a translation unit is a "program"
    fn transpile_translation_unit(this, unit) {
        const forwardDeclarations = this.compiler.create_auxiliary_file("forward_decls.h");
        unit.record_defs.map(fn (r) forwardDeclarations.write(@cpp`class ${r.name};`));
        // ...

        // then later on let's output classes
        unit.record_defs.map();
    }

    fn write_out_class(recordDef: RecordDef) {
        
    }
}
```
