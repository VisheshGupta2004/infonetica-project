namespace Infonetica.Workflow.Core;

public sealed record Transition(
    string Id,
    string Name,
    IReadOnlyCollection<string> FromStates,
    string ToState,
    bool Enabled = true
); 