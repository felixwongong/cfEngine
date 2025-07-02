using System;

namespace cfEngine
{
    public enum ValidationState : byte { Pending, Success, Failure }
    public interface Validation<out T>
    {
        public ValidationState state { get; }
        public T GetValue();
        public Exception GetException();
    }

    public readonly struct Success<T> : Validation<T>
    {
        public ValidationState state => ValidationState.Success;
        
        private readonly T _value;
        public Success(T value)
        {
            _value = value;
        }
        
        public T GetValue() => _value;
        public Exception GetException() => null;
    }
    
    public readonly struct Failure<T> : Validation<T>
    {
        public ValidationState state => ValidationState.Failure;
        public T GetValue() => default;
        
        private readonly Exception _exception;
        public Failure(Exception exception)
        {
            _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        }
        
        public Exception GetException() => _exception;
    }

    public readonly struct Pending<T> : Validation<T>
    {
        public ValidationState state => ValidationState.Pending;
        private readonly T _value;
        public Pending(T value)
        {
            _value = value;
        }
        public T GetValue() => _value;
        public Exception GetException() => null;
    }
}