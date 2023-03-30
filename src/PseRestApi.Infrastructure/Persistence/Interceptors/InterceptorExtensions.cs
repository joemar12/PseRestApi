using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PseRestApi.Infrastructure.Persistence.Interceptors;

public static class InterceptorExtensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(x =>
            x.TargetEntry != null &&
            x.TargetEntry.Metadata.IsOwned() &&
            (x.TargetEntry.State == EntityState.Added || x.TargetEntry.State == EntityState.Modified));
}
