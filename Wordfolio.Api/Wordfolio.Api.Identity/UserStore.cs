using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Wordfolio.Api.Identity;

public class UserStore : UserStore<User, Role, IdentityDbContext, int>
{
    private readonly IUserStoreExtension userStoreExtension;

    public UserStore(IUserStoreExtension userStoreExtension, IdentityDbContext context, IdentityErrorDescriber? describer = null) : base(context, describer)
    {
        this.userStoreExtension = userStoreExtension ?? throw new ArgumentNullException(nameof(userStoreExtension));
    }

    public override async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        var strategy = Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(
            async () =>
            {
                var connection = Context.Database.GetDbConnection();

                await using var contextTransaction =
                    Context.Database.CurrentTransaction
                    ?? await Context.Database.BeginTransactionAsync(cancellationToken);

                await using var transaction = contextTransaction.GetDbTransaction();

                var result = await base.CreateAsync(user, cancellationToken);

                if (user.Id is 0)
                    throw new InvalidOperationException("Expected identity User to be stored in a database before creating Wordfolio User");

                if (result.Succeeded)
                {
                    var userCreationParameters = new UserCreationParameters(Id: user.Id);
                    await userStoreExtension.OnAfterUserCreatedAsync(userCreationParameters, connection, transaction, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }

                return result;
            });
    }
}
