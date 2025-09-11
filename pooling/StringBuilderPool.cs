using System;
using System.Text;

namespace cfEngine.Pooling
{
    public class StringBuilderPool: ObjectPool<StringBuilder>
    {
        public static StringBuilderPool Default { get; } = new();
        
        public StringBuilderPool() : base(_create, static _ => {}, _release, static _ => {})
        {
        }

        private static StringBuilder _create()
        {
            return new StringBuilder();
        }
        
        private static void _release(StringBuilder sb)
        {
            sb.Clear();
        }
    }
}