using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using cfEngine;

namespace cfEngine.Util
{
    public static class SanityCheck
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhenNull<T>(T? target, string message = "") where T: class
        {
            if (target == null)
            {
                Log.LogException(string.IsNullOrEmpty(message)
                    ? new SanityCheckException()
                    : new SanityCheckException(message));
                return true;
            }

            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WhenTrue(bool condition, string message = "")
        {
            if (condition)
            {
                Log.LogException(string.IsNullOrEmpty(message)
                    ? new SanityCheckException()
                    : new SanityCheckException(message));
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Assign<T>(ref T field, T value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)!))
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            field = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Assign<T>(ref T field, T? value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
        {
            if (value is null)
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            field = value.Value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AssignNonEmpty(ref string? field, string? value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            field = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RequireNotNull<T>(T? value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
        {
            if (value is null)
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RequireNotDefault<T>(T value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
        {
            if (value.Equals(default(T)))
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool RequireNonEmpty(string? value,
            string? message = null,
            [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(value))
            {
                Log.LogException(new ArgumentNullException(paramName, message));
                return false;
            }

            return true;
        }
    }
}