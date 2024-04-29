fn fact(n) {
    if n == 0 {
        return 1
    } else {
        return n * fact(n - 1)
    }
}

const console = std.io.console
console.printline(fact(0))
console.printline(fact(1))
console.printline(fact(5))
console.printline(fact(10))
console.printline(fact(15))