using System;
using System.Collections.Generic;

namespace DotNetGB.Util
{
    public readonly struct Optional<T>
        where T : class
    {
        private readonly T? _value;

        private Optional(T value)
        {
            _value = value;
        }

        public static Optional<T> Empty() => new Optional<T>();

        public static Optional<T> Of(T value) => value != null ? new Optional<T>(value) : throw new InvalidOperationException("Optional.Of requires a non-null value.");

        public static Optional<T> OfNullable(T? value) => value != null ? new Optional<T>(value) : new Optional<T>();

        public bool Equals(Optional<T> other) => EqualityComparer<T>.Default.Equals(_value, other._value);

        public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

        public override int GetHashCode() => _value != null ? EqualityComparer<T>.Default.GetHashCode(_value) : 0;

        public static implicit operator Optional<T>(T value) => new Optional<T>(value);

        public Optional<T> Filter(Predicate<T> predicate) => _value != null && predicate(_value) ? this : Empty();

        public Optional<U> FlatMap<U>(Func<T, Optional<U>> mapper)
            where U : class =>
            _value != null ? mapper(_value) : Optional<U>.Empty();

        public T Get() => _value ?? throw new InvalidOperationException("This optional is empty");

        public void IfPresent(Action<T> consumer)
        {
            if (_value != null)
                consumer(_value);
        }

        public bool IsPresent => _value != null;

        public Optional<U> Map<U>(Func<T, U> mapper)
            where U : class =>
            _value != null ? mapper(_value) ?? Optional<U>.Empty() : Optional<U>.Empty();

        public T OrElse(T other) => _value ?? other;

        public T OrElseGet(Func<T> other) => _value ?? other();

        public T OrElseThrow(Action exceptionSupplier)
        {
            if (_value != null) 
                return _value;

            exceptionSupplier();
            throw new InvalidOperationException("Exception supplier did not throw an exception");
        }

        public override string ToString() => _value?.ToString() ?? "{empty}";
    }
}
