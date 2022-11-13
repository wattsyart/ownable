using System.Text;

namespace ownable.Models;

public sealed class QueryStore
{
    public string CreateContinuationToken(string queryHash, Query query)
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);
        query.Serialize(bw);
        var data = ms.ToArray();

        var buffer = Encoding.UTF8.GetBytes(queryHash).Concat(Encoding.UTF8.GetBytes("::")).Concat(data).ToArray();
        var continuationToken = Convert.ToBase64String(buffer);
        return continuationToken;
    }

    public Query GetQuery(string continuationToken)
    {
        var queryString = Encoding.UTF8.GetString(Convert.FromBase64String(continuationToken));
        var tokens = queryString.Split("::", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var serialized = Encoding.UTF8.GetBytes(tokens[1]);

        var ms = new MemoryStream(serialized);
        var br = new BinaryReader(ms);
        var query = new Query();
        query.Deserialize(br);
        return query;
    }
}