using System;
using System.Collections.Generic;

namespace cfEngine.GameItem;

public interface IGameItem { }

public static partial class GameItemFactory
{
    public delegate IGameItem Decoder(ReadOnlySpan<string> args);
    
    private static readonly Dictionary<string, Decoder> _decoders = new();
    
    static GameItemFactory() 
    {
        RegisterDecoders();
    }
    
    static partial void RegisterDecoders();
    
    public static void Register(string typeName, Decoder decoder)
    {
        if (string.IsNullOrEmpty(typeName) || decoder == null)
            throw new ArgumentException("Type name and decoder cannot be null or empty.");

        if (!_decoders.TryAdd(typeName, decoder))
            throw new InvalidOperationException($"Decoder for type '{typeName}' is already registered.");
    }
}