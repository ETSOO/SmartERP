using com.etsoo.Utils.Storage;

namespace Platform.Server.Services
{
    public interface IStorageFactory
    {
        ValueTask<IStorage> CreateAsync(int? orgId, CancellationToken cancellationToken = default);
        int GetOrgIdFromPath(string path);
        string GetOrgPath(int orgId, string folder);
    }
}