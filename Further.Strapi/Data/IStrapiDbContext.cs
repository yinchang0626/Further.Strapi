using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Further.Strapi.Data;

[ConnectionStringName(StrapiDbProperties.ConnectionStringName)]
public interface IStrapiDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
