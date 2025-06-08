using System;
using System.Runtime.CompilerServices;

namespace cfEngine
{
    public partial struct Res<T, TE>
    {
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<T, TE> Ok(T value) => new(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Res<T, TE> Err(TE error) => new(error);
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
    }
}