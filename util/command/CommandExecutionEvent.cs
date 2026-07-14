using System;

namespace cfEngine.Command
{
    public record CommandExecutionEvent(
        string Command,
        bool Success,
        string? Message = null,
        Exception? Exception = null
    );
}
