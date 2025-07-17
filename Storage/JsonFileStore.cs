using System.Text.Json;
using Infonetica.Workflow.Core;
using Infonetica.Workflow.Logic;

namespace Infonetica.Workflow.Storage;

public static class JsonFileStore
{
    private sealed record Snapshot(
        List<WorkflowTemplate> Templates,
        List<WorkflowSession> Sessions
    );

    public static string ExportJson(WorkflowManager mgr)
    {
        var snap = new Snapshot(mgr.ListTemplates().ToList(), mgr.ListSessions().ToList());
        return JsonSerializer.Serialize(snap, new JsonSerializerOptions { WriteIndented = true });
    }

    public static (bool Ok, string? Error) ImportJson(string json, WorkflowManager mgr)
    {
        Snapshot? snap;
        try
        {
            snap = JsonSerializer.Deserialize<Snapshot>(json);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
        if (snap == null) return (false, "Invalid snapshot.");

        foreach (var t in snap.Templates)
        {
            mgr.CreateTemplate(t.Id, t.Name, t.States.Values, t.Transitions.Values);
        }
        foreach (var s in snap.Sessions)
        {
            if (!mgr.TryGetTemplate(s.TemplateId, out var template)) continue;
            var (session, _) = mgr.StartSession(template.Id, s.Id);
            if (session == null) continue;
            session.CurrentStateId = s.CurrentStateId;
            session.History.Clear();
            session.History.AddRange(s.History);
        }
        return (true, null);
    }
} 