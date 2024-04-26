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

## Signals (Reactivity)

> Sadly, the naming for this can be a bit conflated; for example "Signals" in System Programming, Threading, Godot.

> Signals for this is referring to https://github.com/tc39/proposal-signals which is a reactivity system.

Signals are a powerful abstraction that let you "watch" updates and make dynamic UI components.

A simple example would be this;

```lua
record Score {
    base: number,
    additiveMult: number,
    multiplicativeMult: number,

    // the total score will be base * additiveMult * multiplicativeMult
    fn calculateTotalScore() {
        return base * additiveMult * multiplicativeMult
    }
}
```

For this, if we wanted to have some UI text component update dynamically to show the score as it changes we can use reactivity.  The traditional method would be either to put the UI logic in the Score record or have some sort of global event that checks it or just update it every frame.

This means however, that it can get tricky to handle timing updates, what if the score gets updated 10 times very quickly?  Do you want to show each update separately?  This becomes difficult to do if you are doing a pull based model and very complex logic for a simple class if you are doing push based model (which increases unit testing complexity).

A signal solution would be;

```lua
record Score {
    base: Signal[number] = 0,
    additiveMult: Signal[number] = 1,
    multiplicativeMult: Signal[number] = 1,

    totalScore = Signal.Computed(fn() base * additiveMult * multiplicativeMult),
}

// updating score
var score: Score = {}
// setting up a reactive watcher
score.totalScore.watch(fn(newValue) console.println(newValue))

score.base += 10
score.additiveMult += 5
score.multiplicativeMult *= 1.5

// sometimes you want to "scope" a change so that you can apply multiple at once
Signal.scope(fn() {
    score.base += 10
    score.additiveMult += 5
    score.multiplicativeMult *= 1.5
})
```

## Async

We support asynchronous programming out of the box built ontop of a promise based API.

```rust
const client = std.net.client;

fn main() {
    // all functions automatically propagate "async"
    // so we don't need to make it explicit, instead it becomes inferred based on what you do
    fn getTask() {
        // for example this won't be async because it doesn't wait for completion
        return client.get("/version.json")
    }

    const task1 = getTask()
    const timer = new std.time.Timer()

    // Tasks are what is referred to as an "transparent type", they wrap it with behaviour but just add
    // a sort of "tag" on the object/class, like a decorator
    
    // this will block
    // TODO: I don't love this generic syntax... it's just as ambiguous
    //       but maybe we use {} for idx??
    const version = task1.read[string]()
    console.log(timer.currentTime)

    // if you want to explicitly wait for a series of tasks to complete there are a few ways
    // 1. task.wait, allows taking in a series of tasks (or empty arg functions are fine too)
    const { result1, result2 } = std.task.wait(fn() {
        std.task.wait(task1)
        console.log("This should log before the second one, since this doesn't have to wait")
    }, fn() {
        std.task.wait(getTask())
        console.log("This should log after the first one, since this does have to do an api call")
    })

    // 2. you could "schedule" both by calling both then waiting on just one
    // this is *roughly* the same performance as Task.WhenAll
    const task1 = getTask1()
    const task2 = getTask2()

    const result1 = task1.read[string]()
    const result2 = task2.read[string]()
}
```

### Generics

We have pretty flexible support for generics that matches C#

### Source Generation

Source generation is pretty powerful, let's see how a Task.Wait method could be implemented with it!

```lua
fn wait()

```

## Permission

Through the use of 
