using System;
using System.Diagnostics.CodeAnalysis;

namespace cfEngine
{
    public static class Optional
    {
        public static Optional<T> Some<T>([DisallowNull] T value) where T : class
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return Optional<T>.Some(value);
        }
        
        public static Optional<T> None<T>() where T : class => Optional<T>.None();
    }
    
    public readonly partial struct Optional<T>
    {
        private readonly T _value;
        private readonly bool _hasValue;

        private Optional([DisallowNull] T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
            _hasValue = true;
        }

        public static implicit operator Optional<T>(T value) => Optional<T>.Some(value);

        public static Optional<T> Some([DisallowNull] T value) => new(value);
        public static Optional<T> None() => default;

        public bool TryGetValue(out T value)
        {
            value = _value;
            return _hasValue;
        }
        
        public override string ToString() => _hasValue ? $"Some({_value})" : "None";
    }
}
