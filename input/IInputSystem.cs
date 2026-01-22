using System;
using cfEngine.Rx;
using cfEngine.Service;

namespace cfEngine.Input
{
    /// <summary>
    /// Callback context for input actions
    /// </summary>
    public interface IInputActionContext
    {
        public enum Phase: byte
        {
            Performed,
            Canceled
        }
        
        string actionName { get; }
        Phase phase { get; }
        Res<T, Exception> GetValue<T>();
    }
    
    /// <summary>
    /// Abstract interface to the input system.
    /// Provides access to input events and allows registration of action callbacks by action name.
    /// </summary>
    public interface IInputSystem : IService
    {
        /// <summary>
        /// Relay triggered when any input action is triggered
        /// </summary>
        IRelay<IInputActionContext> onActionTriggered { get; }
        
        /// <summary>
        /// Relay triggered when an input device is lost
        /// </summary>
        IRelay onDeviceLost { get; }
        
        /// <summary>
        /// Relay triggered when an input device is regained
        /// </summary>
        IRelay onDeviceRegained { get; }
        
        /// <summary>
        /// Relay triggered when the control scheme changes
        /// </summary>
        IRelay<string> onControlsChanged { get; }
        
        /// <summary>
        /// Register a callback for a specific action by name
        /// </summary>
        /// <param name="actionName">Name of the action to listen for</param>
        /// <param name="callback">Callback to invoke when action is triggered</param>
        Subscription RegisterAction(string actionName, Action<IInputActionContext> callback);
        
        /// <summary>
        /// Unregister a callback for a specific action by name
        /// </summary>
        /// <param name="actionName">Name of the action to stop listening for</param>
        /// <param name="callback">Callback to remove</param>
        void UnregisterAction(string actionName, Action<IInputActionContext> callback);
        
        /// <summary>
        /// Get the current value of an action by name
        /// </summary>
        /// <typeparam name="T">Type of value to read</typeparam>
        /// <param name="actionName">Name of the action</param>
        /// <returns>Current value of the action</returns>
        T GetActionValue<T>(string actionName) where T : struct;
        
        /// <summary>
        /// Check if an action is currently being performed
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <returns>True if action is being performed, false otherwise</returns>
        bool IsActionPerformed(string actionName);
        
        /// <summary>
        /// Get the current control scheme name
        /// </summary>
        string currentControlScheme { get; }
        
        /// <summary>
        /// Get the current action map name
        /// </summary>
        string currentActionMap { get; }
    }
}
