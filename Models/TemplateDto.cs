using Infonetica.Workflow.Core;

namespace Infonetica.Workflow.Models;

public sealed record TemplateDto(
    string Id,
    string Name,
    IEnumerable<State> States,
    IEnumerable<Transition> Transitions,
    string InitialStateId
); 