namespace Infonetica.Workflow.Core;

public sealed class WorkflowSession
{
    public string Id { get; init; }
    public string TemplateId { get; init; }
    public string CurrentStateId { get; internal set; }
    public List<SessionHistoryEntry> History { get; } = new();

    public WorkflowSession(string id, string templateId, string startStateId)
    {
        Id = id;
        TemplateId = templateId;
        CurrentStateId = startStateId;
    }
} 