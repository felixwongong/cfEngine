using System.Threading;
using cfEngine.Service;

namespace cfEngine.Core
{
    public partial class ServiceName { }
    
    public class Game: ServiceLocator
    {
        private static Game _current;
        public static Game Current => _current;
        public static CancellationToken TaskToken { get; private set; } = CancellationToken.None;
        
        public static void SetCurrent(Game game)
        {
            _current?.Dispose();
            _current = game;
        }
    }
}