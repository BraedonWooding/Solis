-- 0, 1, 1, 2, 3, 5, 8, 13, ...
fn fib(n) {
    if (n == 0) { return 0 }
    if (n <= 2) { return 1 }
    return fib(n - 1) + fib(n - 2)
}

const console = std.io.console
console.printline(fact(5))
console.printline(fact(0))
console.printline(fact(1))
console.printline(fact(2))