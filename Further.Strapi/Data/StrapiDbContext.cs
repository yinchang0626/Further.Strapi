using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Further.Strapi.Data;

[ConnectionStringName(StrapiDbProperties.ConnectionStringName)]
public class StrapiDbContext : AbpDbContext<StrapiDbContext>, IStrapiDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public StrapiDbContext(DbContextOptions<StrapiDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureStrapi();
    }
}
