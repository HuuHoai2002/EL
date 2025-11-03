using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace ThongKe.Shared;

public class PagedListRequest
{
	public int Page { get; set; }
	public int Limit { get; set; }
	public string? Search { get; set; }
	public string? TrangThai { get; set; }
	public string? CreatedBy { get; set; }
	public string? UpdatedBy { get; set; }
	public long? FromDateCreated { get; set; }
	public long? ToDateCreated { get; set; }
	public long? FromDateUpdated { get; set; }
	public long? ToDateUpdated { get; set; }
}

public class RequestDanhMucCon : PagedListRequest
{
	public required string MaKieu { get; set; }
	public string? MaMuc { get; set; }
}

public class PagedListResponse<T>
{
	public int Total { get; set; }
	public int TotalPage { get; set; }
	public int CountItem { get; set; }
	public int Page { get; set; }
	public int Limit { get; set; }
	public List<T> Items { get; set; } = new();
}

public class BaseRequest
{
	public int PageNumber { get; set; } = 1;

	[Range(1, 200)] public int PageSize { get; set; } = 10;

	public string? OrderBy { get; set; }
	public bool Ascending { get; set; } = true;
	public string? Keyword { get; set; }
	public string? CreatedBy { get; set; }

	public DateTime? CreatedFrom { get; set; }
	public DateTime? CreatedTo { get; set; }
	public DateTime? UpdatedFrom { get; set; }
	public DateTime? UpdatedTo { get; set; }
}

public class IdentityRequest : BaseRequest
{
	[Required] public int Id { get; set; }
}

public class DeleteManyRecordRequest
{
	[Required][Length(1, 100)] public List<int> Ids { get; set; } = [];
}

public class PagedResult<T>
{
	public PagedResult()
	{
	}

	public PagedResult(List<T> records, int totalRecords, int pageNumber, int pageSize)
	{
		Records = records;
		Total = totalRecords;
		PageNumber = pageNumber;
		PageSize = pageSize;
	}

	public List<T> Records { get; set; } = [];
	public int PageNumber { get; set; }
	public int PageSize { get; set; }
	public int Total { get; set; }
	public int TotalPages => Total > 0 && PageSize > 0 ? (int)Math.Ceiling(Total / (double)PageSize) : 0;
}

public class CursorResult<T>
{
	public List<T> Records { get; set; } = [];

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, object>? Metadata { get; set; }

	public int? NextCursor { get; set; }
	public bool HasNext { get; set; }
}

public class ApiResponse<T>
{
	public bool Success { get; set; } = true;

	public T Data { get; set; } = default!;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public List<string>? Errors { get; set; }

	public string Message { get; set; } = string.Empty;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public Dictionary<string, object?>? Meta { get; set; }

	public static ApiResponse<T> Ok(T data, string message = "Thành công!", Dictionary<string, object?>? meta = null)
	{
		return new ApiResponse<T> { Success = true, Data = data, Message = message, Meta = meta };
	}

	public static ApiResponse<T> Fail(List<string> errors, bool isException = false)
	{
		var message = errors.Count switch
		{
			> 1 when isException => errors[1],
			_ => errors.First()
		};
		return new ApiResponse<T> { Success = false, Errors = errors, Message = message };
	}

	public static ApiResponse<PagedResult<T>> PagedResult(PagedResult<T>? result)
	{
		return new ApiResponse<PagedResult<T>>
		{
			Success = true,
			Data = result!,
			Message = "Thành công!"
		};
	}

	public static ApiResponse<CursorResult<T>> CursorResult(CursorResult<T>? result)
	{
		return new ApiResponse<CursorResult<T>>
		{
			Success = true,
			Data = result!,
			Message = "Thành công!"
		};
	}
}