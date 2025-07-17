using Infonetica.Workflow.Core;

namespace Infonetica.Workflow.Models;

public sealed class CreateTemplateRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<State>? States { get; set; }
    public List<CreateTransitionDto>? Transitions { get; set; }
}

public sealed class CreateTransitionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> FromStates { get; set; } = new();
    public string ToState { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
} 