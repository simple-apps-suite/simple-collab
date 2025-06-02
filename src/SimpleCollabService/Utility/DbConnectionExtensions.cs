using System.Data.Common;
using System.Runtime.CompilerServices;

namespace SimpleCollabService.Utility;

static class DbConnectionExtensions
{
    static readonly ConditionalWeakTable<DbConnection, IServiceProvider> s_cwt = [];

    public static T WithServiceProvider<T>(this T connection, IServiceProvider provider)
        where T : DbConnection
    {
        s_cwt.AddOrUpdate(connection, provider);
        return connection;
    }

    public static IServiceProvider GetServiceProvider(this DbConnection connection) =>
        s_cwt.TryGetValue(connection, out IServiceProvider? provider)
            ? provider
            : throw new InvalidOperationException("Service provider not available.");
}
