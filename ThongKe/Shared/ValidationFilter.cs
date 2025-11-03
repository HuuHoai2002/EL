using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ThongKe.Shared;

public class ValidationFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		if (context.ModelState.IsValid) return;

		var errors = context.ModelState
				.Where(x => x.Value!.Errors.Count > 0)
				.SelectMany(x => x.Value!.Errors)
				.Select(x => x.ErrorMessage)
				.ToList();

		var response = ApiResponse<string>.Fail(errors);
		context.Result = new BadRequestObjectResult(response);
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}