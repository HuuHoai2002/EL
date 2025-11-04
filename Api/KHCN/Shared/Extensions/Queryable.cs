using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace KHCN.Shared.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        string? orderBy = null,
        bool ascending = true,
        Expression<Func<T, bool>>? filter = null)
    {
        if (filter != null) query = query.Where(filter);

        if (pageNumber <= 0) pageNumber = 1;
        if (pageSize <= 0) pageSize = 10;

        if (!string.IsNullOrEmpty(orderBy))
        {
            var property = typeof(T).GetProperty(orderBy,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
                query = ascending
                    ? query.OrderBy(x => EF.Property<object>(x!, property.Name))
                    : query.OrderByDescending(x => EF.Property<object>(x!, property.Name));
        }

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var totalRecords = 0;
        if (items.Count > 0) totalRecords = await query.CountAsync();
        return new PagedResult<T>(items, totalRecords, pageNumber, pageSize);
    }
}