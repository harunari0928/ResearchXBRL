using System.Collections.Generic;
using System.Linq;

namespace ResearchXBRL.CrossCuttingInterest.Extensions
{
    public static class IAsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<IEnumerable<T>> Chunk<T>(this IAsyncEnumerable<T> source, int chunkSize)
        {
            var partition = new List<T>(chunkSize);
            await foreach (var item in source)
            {
                partition.Add(item);
                if (partition.Count == chunkSize)
                {
                    yield return partition;
                    partition.Clear();
                }
            }

            if (partition.Any()) { yield return partition; }
        }
    }
}
