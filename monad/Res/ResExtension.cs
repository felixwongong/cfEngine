using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace cfEngine
{
    public static class Res
    {
        public static Res<T, Exception> Ok<T>([DisallowNull] T value) => Res<T, Exception>.Ok(value);
        public static Res<T, Exception> Err<T>([DisallowNull] Exception error) => Res<T, Exception>.Err(error);
    }
    
    public partial struct Res<TOk, TErr>
    {
        public Res<T, TErr> Map<T>(Func<TOk, T> mapper)
        {
            return _isOk ? Res<T, TErr>.Ok(mapper(_value)) : Res<T, TErr>.Err(_error);
        }

        public Res<TOk, T> MapErr<T>(Func<TErr, T> mapper)
        {
            return _isOk ? Res<TOk, T>.Ok(_value) : Res<TOk, T>.Err(mapper(_error));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<TOk, TErr> Ok([DisallowNull] TOk value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<TOk, TErr> Err([DisallowNull] TErr error) => new(error);
        public Res<TOk, TErr> OnOk(Action<TOk> action)
        {
            if (_isOk) action(_value);
            return this;
        }

        public Res<TOk, TErr> OnErr(Action<TErr> action)
        {
            if (!_isOk) action(_error);
            return this;
        }

        public Res<TOk, TErr> UnwrapOr(Action onErr)
        {
            if (!_isOk) onErr();
            return this;
        }

        public Res<TOk, TErr> UnwrapOr(TOk defaultValue)
        {
            return _isOk ? this : Res<TOk, TErr>.Ok(defaultValue);
        }
    }
}