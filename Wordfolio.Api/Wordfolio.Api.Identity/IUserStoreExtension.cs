using System.Data;

namespace Wordfolio.Api.Identity;

public record struct UserCreationParameters(int Id);

public interface IUserStoreExtension
{
    Task OnAfterUserCreatedAsync(UserCreationParameters parameters, IDbConnection connection, IDbTransaction? transaction, CancellationToken cancellationToken);
}