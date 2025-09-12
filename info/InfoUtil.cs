using System;
using System.Collections.Generic;

namespace cfEngine.Info;

public static class InfoUtil
{
    public static Type ParseTypeCommand(ReadOnlySpan<char> command, out IReadOnlyList<string> args, char separator = ',', char open = '(', char close = ')')
    {
        var typeName = ParseCommand(command, out args, separator, open, close);
        if (string.IsNullOrEmpty(typeName))
            return null;
        return Type.GetType(typeName);
    }
    
    public static string ParseCommand(ReadOnlySpan<char> command, out IReadOnlyList<string> args, char separator = ',', char open = '(', char close = ')')
    {
        string? type = null;
        List<string> readArgs = null;
        
        int start = 0;
        for (int current = 0; current < command.Length; current++)
        {
            var c = command[current];
            if (c.Equals(open))
            {
                if (type == null)
                {
                    type = command[new Range(start, current)].Trim(' ').ToString();
                    start = current + 1;
                }
            } else if (c.Equals(separator) || (c.Equals(close) && current == command.Length - 1))
            {
                readArgs ??= new List<string>();
                var arg = command[new Range(start, current)].Trim(' ');
                readArgs.Add(arg.ToString());
                start = current + 1;
            } 
        }

        if (readArgs != null)
            args = readArgs;
        else
            args = Array.Empty<string>();
        
        return type.Trim(' ');
    }
}
