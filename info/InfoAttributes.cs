#nullable enable
using System;

namespace cfEngine.Info;

/// <summary>Marks the identity column of an Info class (the sheet key column).</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class InfoKeyAttribute : Attribute { }

/// <summary>Declares constraints on an Info sheet column.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class InfoFieldAttribute : Attribute
{
    /// <summary>Column must be present in the sheet.</summary>
    public bool Required { get; set; }

    /// <summary>Inclusive lower bound for numeric columns; unset (default) = no bound.</summary>
    public double Min { get; set; } = double.NegativeInfinity;

    /// <summary>Inclusive upper bound for numeric columns; unset (default) = no bound.</summary>
    public double Max { get; set; } = double.PositiveInfinity;
}

/// <summary>
/// Marks a column whose values are ids of another Info type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class InfoRefAttribute : Attribute
{
    public Type InfoType { get; }

    public InfoRefAttribute(Type infoType)
    {
        InfoType = infoType ?? throw new ArgumentNullException(nameof(infoType));
    }
}
