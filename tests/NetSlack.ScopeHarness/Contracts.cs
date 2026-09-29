using System.Linq.Expressions;
using NetSlack.Domain.Channels;
using NetSlack.Domain.Workspaces;
namespace NetSlack.Application.Common {
public interface ICurrentUser { Guid UserId { get; } string Email { get; } string DisplayName { get; } bool IsAuthenticated { get; } }
public interface IAppDbContext { FakeSet<Workspace> Workspaces { get; } FakeSet<Channel> Channels { get; } FakeSet<WorkspaceMember> WorkspaceMembers { get; } Task<int> SaveChangesAsync(CancellationToken cancellationToken); }
public abstract class AppException(string message) : Exception(message); public sealed class ConflictException(string message) : AppException(message); public sealed class ForbiddenException(string message) : AppException(message);
}

namespace Microsoft.EntityFrameworkCore { public sealed class DbUpdateException(string message, Exception inner) : Exception(message, inner); }

public sealed class FakeSet<T> : List<T>, IQueryable<T>
{
    private IQueryable<T> Query => this.ToArray().AsQueryable(); Type IQueryable.ElementType => typeof(T); Expression IQueryable.Expression => Query.Expression; IQueryProvider IQueryable.Provider => Query.Provider;
}
namespace Microsoft.EntityFrameworkCore { public static class AsyncExtensions { public static Task<bool> AnyAsync<T>(this IQueryable<T> query, Expression<Func<T, bool>> predicate, CancellationToken _) => Task.FromResult(query.Any(predicate.Compile())); public static Task<List<T>> ToListAsync<T>(this IQueryable<T> query, CancellationToken _) => Task.FromResult(query.ToList()); public static IQueryable<T> AsNoTracking<T>(this IQueryable<T> query) => query; } }
