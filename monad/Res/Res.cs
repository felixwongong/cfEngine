using System;
using System.Diagnostics.CodeAnalysis;

namespace cfEngine
{
    public readonly partial struct Res<TOk, TErr>
    {
        private readonly TOk _value;
        private readonly TErr _error;
        private readonly bool _isOk;

        private Res([DisallowNull] TOk result)
        {
            _value = result ?? throw new ArgumentNullException(nameof(result));
            _error = default!;
            _isOk = true;
        }

        private Res([DisallowNull] TErr error)
        {
            _error = error ?? throw new ArgumentNullException(nameof(error));
            _value = default!;
            _isOk = false;
        }

        public bool IsOk => _isOk;
        public bool IsErr => !_isOk;
        public TOk value
        {
            get
            {
                if (!_isOk)
                    throw new InvalidOperationException($"Cannot access Value on an Err result. error: {_error.ToString()}");
                return _value;
            }
        }
        
        public TErr error
        {
            get
            {
                if (_isOk)
                    throw new InvalidOperationException($"Cannot access Error on an Ok result. result: {_value.ToString()}");
                return _error;
            }
        }

        public bool TryGetValue(out TOk value)
        {
            value = _value;
            return _isOk;
        }

        public bool TryGetError(out TErr error)
        {
            error = _error;
            return !_isOk;
        }

        public override string ToString() => _isOk ? $"Ok({_value})" : $"Err({_error})";
    }
}