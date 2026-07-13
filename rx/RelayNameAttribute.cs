using System;

namespace cfEngine.Rx
{
    /// <summary>
    /// Tags a ReactiveProperty<T> or Relay field with a designer-facing name
    /// used by the Rx debugger overlay. Falls back to "Type.FieldName" when absent.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class RelayNameAttribute : Attribute
    {
        public string Name { get; }

        public RelayNameAttribute(string name)
        {
            Name = name ?? string.Empty;
        }
    }
}
