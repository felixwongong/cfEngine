using System;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Fluent validation over values, existing Res results, and deferred funcs.
    /// Rules run in order; the first failure short-circuits into an Err verdict.
    ///
    /// Usage contract:
    ///   value.Validate(rules...).IfTrue(apply).IfFalse(log);        // gate before executing
    ///   Func().Validate(rules...).IfTrue(...).IfFalse(...);         // validate a result
    ///   value.Validate(predicate, code, message).IfTrue(...);       // inline bool lambda
    ///   value.ValidateWith(validator).IfTrue(...);                  // named validator
    ///
    /// Validate never blocks what already happened — it validates whatever it
    /// receives (input, result, or in-flight Res) and everything downstream of
    /// it is conditional. Async chains live in AsyncValidationExtensions; inline
    /// lambdas resolve by return type (ValidationError? vs Task&lt;ValidationError?&gt;),
    /// cast explicitly if a call is ambiguous.
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Runs the rules on value in order. First non-null error fails the
        /// chain; otherwise returns Ok(value).
        /// </summary>
        public static Res<T, ValidationError> Validate<T>(this T value, params ValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            foreach (var rule in rules)
            {
                var error = rule(value);
                if (error != null)
                    return Res<T, ValidationError>.Err(error);
            }

            return Res<T, ValidationError>.Ok(value);
        }

        /// <summary>
        /// Continues a chain: an already-failed Res passes through unchanged;
        /// otherwise the value is validated against the rules.
        /// </summary>
        public static Res<T, ValidationError> Validate<T>(this Res<T, ValidationError> res, params ValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            if (res.IsErr)
                return res;

            return res.value.Validate(rules);
        }

        /// <summary>
        /// Invokes the func, then validates its result.
        /// </summary>
        public static Res<T, ValidationError> Validate<T>(this Func<T> func, params ValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(rules);

            return func().Validate(rules);
        }

        /// <summary>
        /// Inline bool-predicate rule: predicate false produces the given
        /// ValidationError, otherwise Ok(value).
        /// </summary>
        public static Res<T, ValidationError> Validate<T>(this T value, Func<T, bool> predicate, string code, string message)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            return predicate(value)
                ? Res<T, ValidationError>.Ok(value)
                : Res<T, ValidationError>.Err(new ValidationError(code, message));
        }

        /// <summary>
        /// Continues a chain with an inline bool-predicate rule; an already-
        /// failed Res passes through unchanged.
        /// </summary>
        public static Res<T, ValidationError> Validate<T>(this Res<T, ValidationError> res, Func<T, bool> predicate, string code, string message)
        {
            if (res.IsErr)
                return res;

            return res.value.Validate(predicate, code, message);
        }

        /// <summary>
        /// Validates the value through a named validator.
        /// </summary>
        public static Res<T, ValidationError> ValidateWith<T>(this T value, IValidator<T> validator)
        {
            ArgumentNullException.ThrowIfNull(validator);

            var error = validator.Validate(value);
            return error != null
                ? Res<T, ValidationError>.Err(error)
                : Res<T, ValidationError>.Ok(value);
        }

        /// <summary>
        /// Runs the action with the value when the chain is Ok. Returns the Res
        /// so chains compose.
        /// </summary>
        public static Res<T, ValidationError> IfTrue<T>(this Res<T, ValidationError> res, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (res.IsOk)
                action(res.value);

            return res;
        }

        /// <summary>
        /// Runs the action with the error when the chain is Err. Returns the Res
        /// so chains compose.
        /// </summary>
        public static Res<T, ValidationError> IfFalse<T>(this Res<T, ValidationError> res, Action<ValidationError> action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (res.IsErr)
                action(res.error);

            return res;
        }
    }
}
