using System;
using System.Threading;
using System.Threading.Tasks;

namespace cfEngine.Service.Validation
{
    /// <summary>
    /// Async fluent validation over values and Task-based chains. The async
    /// branch exists so a rule can ask a server (AsyncValidationRule) without
    /// changing the chain shape; call sites only add an await. ResolveAsync is
    /// the growth path for server-computed outcomes — the resolver is the
    /// transport call that returns the verdict payload.
    /// </summary>
    public static class AsyncValidationExtensions
    {
        /// <summary>
        /// Pass-through for an async chain with no rules: Ok(value).
        /// </summary>
        public static Task<Res<T, ValidationError>> ValidateAsync<T>(this T value)
        {
            return Task.FromResult(Res<T, ValidationError>.Ok(value));
        }

        /// <summary>
        /// Runs the async rules on value in order. First non-null error fails
        /// the chain; otherwise returns Ok(value).
        /// </summary>
        public static async Task<Res<T, ValidationError>> ValidateAsync<T>(this T value, params AsyncValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            foreach (var rule in rules)
            {
                var error = await rule(value, CancellationToken.None).ConfigureAwait(false);
                if (error != null)
                    return Res<T, ValidationError>.Err(error);
            }

            return Res<T, ValidationError>.Ok(value);
        }

        /// <summary>
        /// Lifts sync rules into an async chain.
        /// </summary>
        public static Task<Res<T, ValidationError>> ValidateAsync<T>(this T value, params ValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(rules);

            return Task.FromResult(value.Validate(rules));
        }

        /// <summary>
        /// Pass-through for an async chain with no rules: the chain's Res.
        /// </summary>
        public static async Task<Res<T, ValidationError>> ValidateAsync<T>(this Task<Res<T, ValidationError>> chain)
        {
            ArgumentNullException.ThrowIfNull(chain);

            return await chain.ConfigureAwait(false);
        }

        /// <summary>
        /// Continues an async chain: an already-failed Res passes through
        /// unchanged; otherwise the value is validated against the async rules.
        /// </summary>
        public static async Task<Res<T, ValidationError>> ValidateAsync<T>(this Task<Res<T, ValidationError>> chain, params AsyncValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(rules);

            var res = await chain.ConfigureAwait(false);
            if (res.IsErr)
                return res;

            return await res.value.ValidateAsync(rules).ConfigureAwait(false);
        }

        /// <summary>
        /// Awaits the task, then validates its result.
        /// </summary>
        public static async Task<Res<T, ValidationError>> ValidateAsync<T>(this Task<T> task, params AsyncValidationRule<T>[] rules)
        {
            ArgumentNullException.ThrowIfNull(task);
            ArgumentNullException.ThrowIfNull(rules);

            return await (await task.ConfigureAwait(false)).ValidateAsync(rules).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs the action with the value when the chain resolves Ok. Returns
        /// the Res so async chains compose.
        /// </summary>
        public static async Task<Res<T, ValidationError>> IfTrue<T>(this Task<Res<T, ValidationError>> chain, Action<T> action)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(action);

            var res = await chain.ConfigureAwait(false);
            if (res.IsOk)
                action(res.value);

            return res;
        }

        /// <summary>
        /// Awaits the action with the value when the chain resolves Ok. Returns
        /// the Res so async chains compose.
        /// </summary>
        public static async Task<Res<T, ValidationError>> IfTrue<T>(this Task<Res<T, ValidationError>> chain, Func<T, Task> action)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(action);

            var res = await chain.ConfigureAwait(false);
            if (res.IsOk)
                await action(res.value).ConfigureAwait(false);

            return res;
        }

        /// <summary>
        /// Runs the action with the error when the chain resolves Err. Returns
        /// the Res so async chains compose.
        /// </summary>
        public static async Task<Res<T, ValidationError>> IfFalse<T>(this Task<Res<T, ValidationError>> chain, Action<ValidationError> action)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(action);

            var res = await chain.ConfigureAwait(false);
            if (res.IsErr)
                action(res.error);

            return res;
        }

        /// <summary>
        /// Awaits the action with the error when the chain resolves Err. Returns
        /// the Res so async chains compose.
        /// </summary>
        public static async Task<Res<T, ValidationError>> IfFalse<T>(this Task<Res<T, ValidationError>> chain, Func<ValidationError, Task> action)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(action);

            var res = await chain.ConfigureAwait(false);
            if (res.IsErr)
                await action(res.error).ConfigureAwait(false);

            return res;
        }

        /// <summary>
        /// Async Bind over the chain: an already-failed Res passes through
        /// unchanged; otherwise the value is transformed through the resolver
        /// (e.g. a server call). The resolver decides the outcome type, so the
        /// chain payload can change type mid-pipeline.
        /// </summary>
        public static async Task<Res<TOut, ValidationError>> ResolveAsync<TIn, TOut>(
            this Task<Res<TIn, ValidationError>> chain,
            Func<TIn, CancellationToken, Task<Res<TOut, ValidationError>>> resolver,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(chain);
            ArgumentNullException.ThrowIfNull(resolver);

            var res = await chain.ConfigureAwait(false);
            if (res.IsErr)
                return Res<TOut, ValidationError>.Err(res.error);

            return await resolver(res.value, ct).ConfigureAwait(false);
        }
    }
}
