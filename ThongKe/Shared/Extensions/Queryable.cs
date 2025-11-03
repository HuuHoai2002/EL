using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ThongKe.Shared.Extensions;

public static class QueryableExtensions
{
    public static PagedListResponse<T> ToPagedListResponse<T>(
        this IQueryable<T> query, int page, int limit)
    {
        var total = query.Count();
        var totalPage = limit > 0 ? (int)Math.Ceiling((double)total / limit) : 1;
        List<T> items;
        if (limit <= 0)
        {
            items = query.ToList();
            page = 1;
            totalPage = 1;
            limit = items.Count;
        }
        else
        {
            items = query.Skip((page - 1) * limit).Take(limit).ToList();
        }

        return new PagedListResponse<T>
        {
            Total = total,
            TotalPage = totalPage,
            CountItem = items.Count,
            Page = page,
            Limit = limit,
            Items = items
        };
    }

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