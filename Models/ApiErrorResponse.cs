namespace Infonetica.Workflow.Models;

public sealed record ApiErrorResponse(string Code, string Message, object? Details = null); 