using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System.Linq.Expressions;

namespace PseRestApi.Test.Helpers.Db;

public static class Utils
{
    public static DbSet<TEntity> BuildMockDbSet<TEntity>(this ICollection<TEntity> data) where TEntity : class
    {
        return BuildMockDbSet<TEntity, TestExpressionVisitor>(data);
    }

    public static DbSet<TEntity> BuildMockDbSet<TEntity, TExpressionVisitor>(ICollection<TEntity> data)
        where TEntity : class
        where TExpressionVisitor : ExpressionVisitor, new()
    {
        var mockSet = Substitute.For<DbSet<TEntity>, IQueryable<TEntity>, IAsyncEnumerable<TEntity>>();

        if (data != null)
        {
            var queryable = data.AsQueryable();
            var enumerable = new TestAsyncEnumerable<TEntity, TExpressionVisitor>(data, entity => data.Remove(entity));

            mockSet.GetAsyncEnumerator(Arg.Any<CancellationToken>()).Returns(args => enumerable.GetAsyncEnumerator());

            ((IQueryable<TEntity>)mockSet).Provider.Returns(enumerable);
            ((IQueryable<TEntity>)mockSet).Expression.Returns(queryable.Expression);
            ((IQueryable<TEntity>)mockSet).ElementType.Returns(queryable.ElementType);
            ((IQueryable<TEntity>)mockSet).GetEnumerator().Returns(info => data.GetEnumerator());
            
            mockSet.AsAsyncEnumerable().Returns(args => CreateAsyncMock(data));
        }

        mockSet.AsQueryable().Returns(mockSet);
        return mockSet;
    }

    private static async IAsyncEnumerable<TEntity> CreateAsyncMock<TEntity>(IEnumerable<TEntity> data)
          where TEntity : class
    {
        foreach (var entity in data)
        {
            yield return entity;
        }

        await Task.CompletedTask;
    }
}
