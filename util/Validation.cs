using System;

namespace cfEngine.Util
{
    public enum ValidationState: byte
    {
        Success,
        Failure,
    }

    public sealed class Validation<T>
    {
        public ValidationState State;
        public T Result;
        public Exception Exception;

        public static Validation<T> Success(T result)
        {
            return new Validation<T>()
            {
                State = ValidationState.Success,
                Result = result,
                Exception = null
            };
        }

        public static Validation<T> Failure(Exception exception)
        {
            return new Validation<T>()
            {
                State = ValidationState.Failure,
                Result = default,
                Exception = exception
            };
        }
    }
}