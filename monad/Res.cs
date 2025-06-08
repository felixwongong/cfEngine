using System;
using System.Runtime.CompilerServices;

namespace cfEngine
{
    public readonly partial struct Res<T, TE>
    {
        private readonly T _value;
        private readonly TE _error;
        private readonly bool _isOk;

        private Res(T value)
        {
            _value = value;
            _error = default!;
            _isOk = true;
        }

        private Res(TE error)
        {
            _error = error;
            _value = default!;
            _isOk = false;
        }

        public bool IsOk => _isOk;
        public bool IsErr => !_isOk;

        public T Value => _isOk
            ? _value
            : throw new InvalidOperationException("Attempted to access Ok value when Result is Err.");

        public TE Error => !_isOk
            ? _error
            : throw new InvalidOperationException("Attempted to access Err value when Result is Ok.");

        public bool TryGetValue(out T value)
        {
            value = _value;
            return _isOk;
        }

        public bool TryGetError(out TE error)
        {
            error = _error;
            return !_isOk;
        }


        public override string ToString() => _isOk ? $"Ok({_value})" : $"Err({_error})";
    }
}