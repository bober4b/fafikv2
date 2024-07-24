
namespace Fafikv2.Data.DifferentClasses
{
    public static class EnumerableExtensions
    {
        private static readonly Random Random = new();
        public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, int count)
        {
            return source.OrderBy(_ => Random.Next()).Take(count);
        }
    }
}
