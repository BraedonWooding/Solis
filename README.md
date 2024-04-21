# Solis

> A language inspired by Lua for simple game/program scripting
> By Braedon Wooding

If you know Lua you'll be able to learn this all very quickly!

```lua
-- There is no concept of "imports" you just specify the path
const console = std.io.console

-- typing is required for function arguments
fn fact(n: int): int {
    if n == 0 {
        return 1
    } else {
        return n * fact(n - 1)
    }
}

-- some functions have changed so you'll have to refamiliarize yourself with them
console.print("enter a number: ")
-- we have pretty good inferred typing
const a = int.parse(console.readline())
console.printline(fact(a))
```

## Goals

- Game development
  - Built to be integrated into C# game engines
- Scripting
  - Anywhere CLR is, you can use it to write simple scripts, we support outputting EXEs given that we can generate C#
- Sandboxing
  - We support restricting what libraries to load
  - We have a cool permission based sandboxing mode for the edge-cases where you want to restrict apis behind a "secure" permission approval.
- Performance
  - Standard execution is an interpreter mode written in C#
    - This isn't written to be performant
  - We support compiling to C# code now which will run much faster (will require a runtime c# compiler though like Rosyln)
    - Since we are fully typed we output without dynamic types
  - In the future we may support compiling directly to IL code
- Typing
  - We support a simple typing system that rougly matches C#
  - We support a few extra typing structures that C# doesn't currently support the main ones being;
    - Tagged Union types (also called Abstract Data Types)
- Async
  - We support async/await through C# tasks

## Solis vs Lua

I decided early on not to support any Lua syntax since I wanted full typing (for both performance & simplicitity of implementation).

### Small syntax stuff

TODO

### Imports

> This is much simpler than Lua for an end-user

If they are standard library/global modules that already are loaded then you don't need to do an explicit require just use it as is i.e. `std.io.console.printline("X")`.  To load a library/local file you use the import syntax.

```lua
-- standard "require" aren't required
const console = std.io.console

-- imports are done in code but rather specified as part of build and are accessed as if you are accessing from root file
--[[
    i.e. if you have the following structure

    lib
        text
            json.sol
            xml.sol
    src
        animal
            cat.sol (this file)
            dog.sol
--]]
-- to access json.sol you do
const json = lib.text.json

-- to access "dog" you can either do absolute
const dog_absolute = src.animal.dog
-- or you can omit src (allowed for all src)
const dog_omitsrc = animal.dog
-- and finally you can do a "local" access
const dog_local = dog
```

#### Lambda

```lua
const add = fn(a, b) { return a + b };
-- you can also elide the { } if the value is a single expression i.e.
const add5 = fn(a) add(a, 5);
```

#### Modules

We don't actually have any module support, because it's not required!  By default all functions/objects are accessible and you can hide them simply by just prefixing it with `_` for example.

```lua
-- json.sol

-- can be accessed through json.parse
fn parse(input: string): json {
    -- ...
}

-- not accessible
fn _underlying_function() {

}
```

### Async

TODO:

### Types

By default we support the following types



### Collections

> This is where we have a little more complexity than Lua but we still keep most types simple!

```lua
-- You can define a table/map (in this case it is string -> bool) like this
const my_map = new Map{
    "A": true,
    "B": false
}

-- You can define a list/array like this
const my_list = new List{
    "A", "B", "C"
}

-- There are quite a few other types such as sets/bitmaps/... using similar constructors
-- to define the type explicitly you use the `[...]` syntax
const explicit_map = new Map[string, bool]{
    "A": true,
    "B": false
}
```

### Custom Types

If you wish to define a custom type for example a 3d point type you could define it like this

```lua
record Point {
    x: number,
    y: number
}
```

## C# Integration

You can export any function/class to C# through the use of an attribute.

```cs
// will by default apply to all public methods/properties/fields
[SolisExport]
public class MyClass {
    // won't apply to the setter since that's private
    public string X { get; private set; }
    public int Y;

    [SolisHide]
    public void HideThisMethod() {
    }
}
```

## Permission

Through the use of 
