-- 0, 1, 1, 2, 3, 5, 8, 13, ...
fn fib(n: int): int {
    if (n == 0) { return 0 }
    if (n <= 2) { return 1 }
    return fib(n - 1) + fib(n - 2)
}

const console = std.io.console
console.printline(fib(5))
console.printline(fib(0))
console.printline(fib(1))
console.printline(fib(2))
console.printline(fib(20))