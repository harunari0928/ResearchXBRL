using System.Linq;
using Xunit;
using ResearchXBRL.CrossCuttingInterest.Extensions;
using System.Threading.Tasks;

namespace ResearchXBRL.Tests.CrossCuttingInterest.Extensions
{
    public sealed class IAsyncEnumerableExtensionsTests
    {
        [Fact]
        public async Task 指定した数のチャンク分けを行う()
        {
            // arrange
            var list = Enumerable.Range(0, 10).OrderBy(x => x).ToAsyncEnumerable();

            // act
            var chunkedList = list.Chunk(3);

            // assert
            // 0 ~ 9 の10この数字が4つのチャンクに分ける
            var firstChunk = await chunkedList.ElementAtAsync(0);
            Assert.True(firstChunk.SequenceEqual(new int[] { 0, 1, 2 }));

            var secondChunk = await chunkedList.ElementAtAsync(1);
            Assert.True(secondChunk.SequenceEqual(new int[] { 3, 4, 5 }));

            // 4つめのチャンクには1つしか数字がない
            var lastChunk = await chunkedList.LastAsync();
            Assert.True(lastChunk.SequenceEqual(new int[] { 9 }));
        }
    }
}
