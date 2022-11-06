using LightningDB;
using System.Text;
using ownable.Models;

namespace ownable
{
    public class Store
    {
        private readonly LightningEnvironment _env;

        public Store()
        {
            var configuration = new EnvironmentConfiguration { MapSize = 10_485_760 };
            _env = new LightningEnvironment("store", configuration);
            _env.MaxDatabases = 1;
            _env.Open();
        }

        public void SetTokenType(string contractAddress, string tokenType)
        {
            using var tx = _env.BeginTransaction();
            using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.Create });

            tx.Put(db, Encoding.UTF8.GetBytes($"C:K:{contractAddress}"), Encoding.UTF8.GetBytes(contractAddress), PutOptions.NoOverwrite);
            tx.Put(db, Encoding.UTF8.GetBytes($"C:T:{contractAddress}"), Encoding.UTF8.GetBytes(tokenType), PutOptions.NoOverwrite);
            tx.Commit();
        }

        public IEnumerable<Contract> GetContracts(CancellationToken cancellationToken)
        {
            using var tx = _env.BeginTransaction(TransactionBeginFlags.ReadOnly);
            using var db = tx.OpenDatabase(configuration: new DatabaseConfiguration { Flags = DatabaseOpenFlags.None });
            using var cursor = tx.CreateCursor(db);

            var entries = new Dictionary<string, Contract>();

            {
                var prefix = Encoding.UTF8.GetBytes("C:K:");
                var sr = cursor.SetRange(prefix);
                if (sr != MDBResultCode.Success)
                    return entries.Values;

                var (r, k, v) = cursor.GetCurrent();

                while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
                {
                    var key = k.AsSpan();

                    if (!key.StartsWith(prefix))
                        break;

                    var index = v.AsSpan();
                    if (index.Length == 0)
                        break;

                    var entry = new Contract();
                    entry.Address = Encoding.UTF8.GetString(index);
                    entries.Add(entry.Address, entry);

                    r = cursor.Next();
                    if (r == MDBResultCode.Success)
                        (r, k, v) = cursor.GetCurrent();
                }
            }

            foreach(var entry in entries)
            {
                var prefix = Encoding.UTF8.GetBytes($"C:T:{entry.Key}");
                var sr = cursor.SetRange(prefix);
                if (sr != MDBResultCode.Success)
                    return entries.Values;

                var (r, k, v) = cursor.GetCurrent();

                while (r == MDBResultCode.Success && !cancellationToken.IsCancellationRequested)
                {
                    var key = k.AsSpan();
                    if (!key.StartsWith(prefix))
                        break;

                    var index = v.AsSpan();
                    if (index.Length == 0)
                        break;

                    entry.Value.Type = Encoding.UTF8.GetString(index);
                    r = cursor.Next();
                    if (r == MDBResultCode.Success)
                        (r, k, v) = cursor.GetCurrent();
                }
            }

            return entries.Values;
        }
    }
}
