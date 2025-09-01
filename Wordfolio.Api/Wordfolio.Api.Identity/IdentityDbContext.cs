using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wordfolio.Api.Identity;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : IdentityDbContext<User, Role, int>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema(Constants.SchemaName);
    }
}