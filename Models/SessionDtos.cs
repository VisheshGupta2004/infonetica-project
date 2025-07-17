using Infonetica.Workflow.Core;

namespace Infonetica.Workflow.Models;

public sealed record StartSessionRequest(string TemplateId, string? Id = null);

public sealed record SessionDto(
    string Id,
    string TemplateId,
    string CurrentState,
    bool IsFinal,
    IReadOnlyList<SessionHistoryEntry> History
); 