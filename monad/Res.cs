using System;
using System.Runtime.CompilerServices;

namespace cfEngine.Monad
{
    public readonly struct Res<T, TE>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<T, TE> Ok(T value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<T, TE> Err(TE error) => new(error);

        public Res<U, TE> Map<U>(Func<T, U> mapper)
        {
            return _isOk ? Res<U, TE>.Ok(mapper(_value)) : Res<U, TE>.Err(_error);
        }

        public Res<T, UE> MapErr<UE>(Func<TE, UE> mapper)
        {
            return _isOk ? Res<T, UE>.Ok(_value) : Res<T, UE>.Err(mapper(_error));
        }

        public Res<U, TE> Bind<U>(Func<T, Res<U, TE>> binder)
        {
            return _isOk ? binder(_value) : Res<U, TE>.Err(_error);
        }

        public R Match<R>(Func<T, R> ok, Func<TE, R> err)
        {
            return _isOk ? ok(_value) : err(_error);
        }

        public Res<T, TE> OnOk(Action<T> action)
        {
            if (_isOk) action(_value);
            return this;
        }

        public Res<T, TE> OnErr(Action<TE> action)
        {
            if (!_isOk) action(_error);
            return this;
        }

        public override string ToString() => _isOk ? $"Ok({_value})" : $"Err({_error})";
    }
}