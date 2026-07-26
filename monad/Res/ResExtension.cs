using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace cfEngine
{
    public static class Res
    {
        public static Res<T, Exception> Ok<T>([DisallowNull] T value) => Res<T, Exception>.Ok(value);
        public static Res<T, Exception> Err<T>([DisallowNull] Exception error) => Res<T, Exception>.Err(error);

        public static Res<(T1, T2), TErr> Combine<T1, T2, TErr>(Res<T1, TErr> a, Res<T2, TErr> b)
        {
            if (a.HasError(out var error)) return Res<(T1, T2), TErr>.Err(error);
            if (b.HasError(out error)) return Res<(T1, T2), TErr>.Err(error);
            return Res<(T1, T2), TErr>.Ok((a.value, b.value));
        }

        public static Res<(T1, T2, T3), TErr> Combine<T1, T2, T3, TErr>(
            Res<T1, TErr> a, Res<T2, TErr> b, Res<T3, TErr> c)
        {
            if (a.HasError(out var error)) return Res<(T1, T2, T3), TErr>.Err(error);
            if (b.HasError(out error)) return Res<(T1, T2, T3), TErr>.Err(error);
            if (c.HasError(out error)) return Res<(T1, T2, T3), TErr>.Err(error);
            return Res<(T1, T2, T3), TErr>.Ok((a.value, b.value, c.value));
        }

        public static Res<(T1, T2, T3, T4), TErr> Combine<T1, T2, T3, T4, TErr>(
            Res<T1, TErr> a, Res<T2, TErr> b, Res<T3, TErr> c, Res<T4, TErr> d)
        {
            if (a.HasError(out var error)) return Res<(T1, T2, T3, T4), TErr>.Err(error);
            if (b.HasError(out error)) return Res<(T1, T2, T3, T4), TErr>.Err(error);
            if (c.HasError(out error)) return Res<(T1, T2, T3, T4), TErr>.Err(error);
            if (d.HasError(out error)) return Res<(T1, T2, T3, T4), TErr>.Err(error);
            return Res<(T1, T2, T3, T4), TErr>.Ok((a.value, b.value, c.value, d.value));
        }

        public static Res<(T1, T2, T3, T4, T5), TErr> Combine<T1, T2, T3, T4, T5, TErr>(
            Res<T1, TErr> a, Res<T2, TErr> b, Res<T3, TErr> c, Res<T4, TErr> d, Res<T5, TErr> e)
        {
            if (a.HasError(out var error)) return Res<(T1, T2, T3, T4, T5), TErr>.Err(error);
            if (b.HasError(out error)) return Res<(T1, T2, T3, T4, T5), TErr>.Err(error);
            if (c.HasError(out error)) return Res<(T1, T2, T3, T4, T5), TErr>.Err(error);
            if (d.HasError(out error)) return Res<(T1, T2, T3, T4, T5), TErr>.Err(error);
            if (e.HasError(out error)) return Res<(T1, T2, T3, T4, T5), TErr>.Err(error);
            return Res<(T1, T2, T3, T4, T5), TErr>.Ok((a.value, b.value, c.value, d.value, e.value));
        }
    }
    
    public partial struct Res<TOk, TErr>
    {
        public Res<T, TErr> Bind<T>(Func<TOk, Res<T, TErr>> binder)
        {
            return _isOk ? binder(_value) : Res<T, TErr>.Err(_error);
        }

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