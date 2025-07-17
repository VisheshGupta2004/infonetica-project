namespace Infonetica.Workflow.Core;

public sealed record SessionHistoryEntry(
    DateTimeOffset Timestamp,
    string TransitionId,
    string FromStateId,
    string ToStateId
); 