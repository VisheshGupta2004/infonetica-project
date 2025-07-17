namespace Infonetica.Workflow.Core;

public sealed record State(
    string Id,
    string Name,
    bool IsInitial = false,
    bool IsFinal = false,
    bool Enabled = true
); 