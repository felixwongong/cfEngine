using System;
using System.Collections.Generic;
using System.IO;
using cfEngine.Pooling;

namespace cfEngine.Modules.cfEngine.util
{
    public struct PathSegment
    {
        private const char CustomSeparator = '/';

        private ReadOnlyMemory<string> _segments;

        public PathSegment(ReadOnlyMemory<string> segments)
        {
            _segments = segments;
        }

        public readonly bool HasValue()
        {
            return _segments.Length > 0;
        }

        public readonly string GetPath()
        {
            if (!HasValue())
                return string.Empty;

            using var handle = StringBuilderPool.Default.Get(out var sb);
            foreach (var s in _segments.Span)
            {
                sb.Append(CustomSeparator);
                sb.Append(s);
            }

            return sb.ToString();
        }

        public readonly string GetOsPath()
        {
            if (!HasValue())
                return string.Empty;

            using var handle = StringBuilderPool.Default.Get(out var sb);
            foreach (var s in _segments.Span)
            {
                sb.Append(Path.PathSeparator);
                sb.Append(s);
            }

            return sb.ToString();
        }

        public ReadOnlyMemory<string> GetSegments()
        {
            return _segments;
        }

        public override string ToString() => GetPath();
    }
    
    public class CustomPathBuilder: IDisposable
    {
        private List<string> sb;

        public CustomPathBuilder()
        {
            sb = ListPool<string>.Default.Get();
        }

        public CustomPathBuilder AppendPath(string path)
        {
            sb.Add(path);
            return this;
        }

        public CustomPathBuilder AppendPath(PathSegment pathSegment)
        {
            foreach (var segment in pathSegment.GetSegments().Span)
            {
                sb.Add(segment);
            }
            return this;
        }

        public PathSegment Build()
        {
            return new PathSegment(sb.ToArray().AsMemory());
        }

        public void Dispose()
        {
            ListPool<string>.Default.Release(sb);
        }
    }
}