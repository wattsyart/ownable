using Nethereum.Hex.HexTypes;
using ownable.Models;

namespace ownable.tests
{
    public class QueryStoreTests
    {
        [Fact]
        public void ContinuationTokenTests()
        {
            var store = new QueryStore();
            var continuationToken = store.CreateContinuationToken("ThisIsMyHash", new Query {FilterId = new HexBigInteger(12345)});
            var query = store.GetQuery(continuationToken);
            Assert.NotNull(query); 
            Assert.Equal(12345, query.FilterId.Value);
        }
    }
}
