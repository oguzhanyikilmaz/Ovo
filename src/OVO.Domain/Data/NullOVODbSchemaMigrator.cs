using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace OVO.Data;

/* This is used if database provider does't define
 * IOVODbSchemaMigrator implementation.
 */
public class NullOVODbSchemaMigrator : IOVODbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
