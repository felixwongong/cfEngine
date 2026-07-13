#if CF_RX_INSTRUMENTED

using System;

namespace cfEngine.Rx
{
    /// <summary>
    /// Static hook points for Relay/Reactive dispatch instrumentation.
    /// All delegates are null by default; an external recorder (e.g. RxRecorder)
    /// assigns them to receive events. Null checks mean zero overhead when no
    /// recorder is attached.
    ///
    /// This class is in cfEngine (pure C#, no Godot deps). The recorder that
    /// subscribes lives in CatSweeper and may reference Godot.
    /// </summary>
    public static class RxInstrumentation
    {
        /// <summary>
        /// If true, exceptions thrown inside a dispatch listener are re-thrown
        /// after being logged (preserving current behavior). If false (default),
        /// exceptions are caught, reported via OnDispatched, and the dispatch
        /// loop continues with the next listener.
        /// </summary>
        public static bool StrictMode;

        /// <summary> Fired after a listener is added to a dispatcher. (owner, listener, subscriptionWeakRef) </summary>
        public static Action<object?, Delegate?, WeakReference<Subscription>?>? OnListenerSubscribed;

        /// <summary> Fired after a listener is removed from a dispatcher. (owner, listener) </summary>
        public static Action<object?, Delegate?>? OnListenerRemoved;

        /// <summary> Fired immediately before a listener is invoked. (owner, listener, args) </summary>
        public static Action<object?, Delegate?, object[]?>? OnDispatching;

        /// <summary>
        /// Fired immediately after a listener invocation completes (success or exception).
        /// (owner, listener, elapsedMicroseconds, exceptionOrNull)
        /// </summary>
        public static Action<object?, Delegate?, long, Exception?>? OnDispatched;
    }
}

#endif
