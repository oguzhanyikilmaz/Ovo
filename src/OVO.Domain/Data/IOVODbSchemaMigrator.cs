using System.Threading.Tasks;

namespace OVO.Data;

public interface IOVODbSchemaMigrator
{
    Task MigrateAsync();
}
