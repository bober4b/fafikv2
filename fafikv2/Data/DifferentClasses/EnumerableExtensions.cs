
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
    public static class GuidExtensions
    {
        public static Guid ToGuid(this ulong value) => Guid.Parse($"{value:X32}");
    }

}
