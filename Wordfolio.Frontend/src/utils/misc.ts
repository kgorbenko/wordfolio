export function assertNonNullable<T>(
    value: T
): asserts value is NonNullable<T> {
    if (value === null || value === undefined) {
        throw new Error(
            `Expected value to be not null or undefined, but got ${value}`
        );
    }
}

export function ensureNonNullable<T>(value: T): NonNullable<T> {
    assertNonNullable(value);
    return value;
}
