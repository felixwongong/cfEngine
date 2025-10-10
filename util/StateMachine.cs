using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using cfEngine.Logging;
using cfEngine.Rx;

namespace cfEngine.Util
{
    public struct StateChangeRecord<TStateId>
    {
        public TStateId LastState;
        public TStateId NewState;
    }

    public interface IStateMachine<TStateId>
    {
        public TStateId lastStateId { get; }
        public TStateId currentStateId { get; }
        public bool CanGoToState(TStateId id, StateParam param = null);
        public bool TryGoToState(TStateId nextStateId, StateParam param = null);
        public void ForceGoToState(TStateId nextStateId, StateParam param = null);
        public Subscription SubscribeBeforeStateChange(Action<StateChangeRecord<TStateId>> listener);
        public Subscription SubscribeAfterStateChange(Action<StateChangeRecord<TStateId>> listener);
    }
    
    public class StateExecutionException<TStateId> : Exception
    {
        public StateExecutionException(TStateId stateId, Exception innerException): base($"State {stateId} execution failed", innerException)
        {
        }
    }
    
    public class StateParam
    {
    }

    public class StateMachine<TStateId, TState, TStateMachine>: IStateMachine<TStateId>, IDisposable
        where TStateMachine: StateMachine<TStateId, TState, TStateMachine>
        where TState: State<TStateId, TState, TStateMachine>
    {
        private TState _lastState;
        private TState _currentState;
        public TStateId lastStateId => _lastState.id;
        public TStateId currentStateId => _currentState.id;
        
        private readonly Dictionary<TStateId, TState> _stateDictionary = new();

        #region Relay & Events (OnBeforeStateChange[Once], OnAfterStateChange[Once]);

        private Relay<StateChangeRecord<TStateId>> _beforeStateChangeRelay;
        public Subscription SubscribeBeforeStateChange(Action<StateChangeRecord<TStateId>> listener)
        {
            _beforeStateChangeRelay ??= new Relay<StateChangeRecord<TStateId>>(this);
            return _beforeStateChangeRelay.AddListener(listener);
        }
        
        private Relay<StateChangeRecord<TStateId>> _afterStateChangeRelay;

        public Subscription SubscribeAfterStateChange(Action<StateChangeRecord<TStateId>> listener)
        {
            _afterStateChangeRelay ??= new Relay<StateChangeRecord<TStateId>>(this);
            return _afterStateChangeRelay.AddListener(listener);
        }

        #endregion

        public StateMachine()
        {
        }

        public virtual void RegisterState([NotNull] TState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (!_stateDictionary.TryAdd(state.id, state))
            {
                throw new Exception($"State {state.GetType()} already registered");
            }

            state.stateMachine = (TStateMachine)this;
        }
        
        public bool CanGoToState(TStateId id, StateParam param)
        {
            return TryGetState(id, out var nextState) && (nextState.IsReady() || _currentState == null);
        }

        public bool TryGoToState(TStateId nextStateId, StateParam param = null)
        {
            try
            {
                if (!TryGetState(nextStateId, out var nextState))
                {
                    Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                    return false;
                }

                if (!CanGoToState(nextState.id, param))
                {
                    Log.LogException(new ArgumentException($"Cannot go to state {nextState.id}"));
                    return false;
                }

                if (_currentState != null)
                {
                    _beforeStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _currentState.id, NewState = nextState.id });
                
                    _currentState.OnEndContext();
                    _lastState = _currentState;
                }
            
                _currentState = nextState;
                if (_lastState != null)
                {
                    _afterStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _lastState.id, NewState = _currentState.id });
                }
                _currentState.StartContext(param);

                return true;
            }
            catch (Exception e)
            {
                Log.LogException(e);
                return false;
            }
        }

        public void ForceGoToState(TStateId nextStateId, StateParam param = null)
        {
            try
            {
                if (!TryGetState(nextStateId, out var nextState))
                {
                    Log.LogException(new KeyNotFoundException($"State {nextStateId} not registered"));
                    return;
                }

                if (_currentState != null)
                {
                    _beforeStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _currentState.id, NewState = nextState.id });
                
                    _currentState.OnEndContext();
                    _lastState = _currentState;
                }
            
                _currentState = nextState;
                if (_lastState != null)
                {
                    _afterStateChangeRelay?.Dispatch(new StateChangeRecord<TStateId>
                        { LastState = _lastState.id, NewState = _currentState.id });
                }
                _currentState.StartContext(param);
            }
            catch (Exception e)
            {
                Log.LogException(e);
            }
        }

        public TState GetStateUnsafe(TStateId id)
        {
            if (!_stateDictionary.TryGetValue(id, out var state))
            {
                throw new Exception($"State {id} not registered");
            }

            return state;
        }

        public T GetStateUnsafe<T>(TStateId id) where T : TState
        {
            return (T)GetStateUnsafe(id);
        }
        
        public bool TryGetState(TStateId id, out TState state)
        {
            if (!_stateDictionary.TryGetValue(id, out state))
            {
                Log.LogException(new Exception($"State {id} not registered"));
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            foreach (var state in _stateDictionary.Values)
            {
                state.Dispose();
            }
            
            _stateDictionary.Clear();
        }
    }
}