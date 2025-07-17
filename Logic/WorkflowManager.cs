using System.Collections.Concurrent;
using Infonetica.Workflow.Core;

namespace Infonetica.Workflow.Logic;

public sealed class WorkflowManager
{
    private readonly ConcurrentDictionary<string, WorkflowTemplate> _templates = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, WorkflowSession> _sessions = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<WorkflowTemplate> ListTemplates() => _templates.Values.ToList();
    public IReadOnlyCollection<WorkflowSession> ListSessions() => _sessions.Values.ToList();

    public bool TryGetTemplate(string id, out WorkflowTemplate template) => _templates.TryGetValue(id, out template!);
    public bool TryGetSession(string id, out WorkflowSession session) => _sessions.TryGetValue(id, out session!);

    public (WorkflowTemplate? Template, string? Error) CreateTemplate(
        string? id,
        string name,
        IEnumerable<State> states,
        IEnumerable<Transition> transitions)
    {
        var templateId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("n") : id.Trim();

        // basic validation
        var stateList = states?.ToList() ?? new();
        if (stateList.Count == 0) return (null, "Template must contain at least one state.");
        if (stateList.Count(s => s.IsInitial) != 1) return (null, "Template must contain exactly one initial state.");
        var dupState = stateList.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (dupState != null) return (null, $"Duplicate state id: {dupState.Key}");

        var transitionList = transitions?.ToList() ?? new();
        var dupTransition = transitionList.GroupBy(a => a.Id, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
        if (dupTransition != null) return (null, $"Duplicate transition id: {dupTransition.Key}");

        var stateIds = new HashSet<string>(stateList.Select(s => s.Id), StringComparer.OrdinalIgnoreCase);
        foreach (var t in transitionList)
        {
            if (!stateIds.Contains(t.ToState)) return (null, $"Transition '{t.Id}' references unknown ToState '{t.ToState}'.");
            foreach (var fs in t.FromStates)
                if (!stateIds.Contains(fs)) return (null, $"Transition '{t.Id}' references unknown FromState '{fs}'.");
        }

        var template = new WorkflowTemplate(templateId, name, stateList, transitionList);
        if (!_templates.TryAdd(templateId, template)) return (null, $"Template id '{templateId}' already exists.");
        return (template, null);
    }

    public (WorkflowSession? Session, string? Error) StartSession(string templateId, string? id = null)
    {
        if (!TryGetTemplate(templateId, out var template)) return (null, $"Template '{templateId}' not found.");
        var sessionId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("n") : id.Trim();
        if (_sessions.ContainsKey(sessionId)) return (null, $"Session id '{sessionId}' already exists.");
        var session = new WorkflowSession(sessionId, templateId, template.InitialStateId);
        _sessions[sessionId] = session;
        return (session, null);
    }

    public (WorkflowSession? Session, string? Error) ExecuteTransition(string sessionId, string transitionId)
    {
        if (!TryGetSession(sessionId, out var session)) return (null, $"Session '{sessionId}' not found.");
        if (!TryGetTemplate(session.TemplateId, out var template)) return (null, $"Template '{session.TemplateId}' not found.");
        if (!template.Transitions.TryGetValue(transitionId, out var transition)) return (null, $"Transition '{transitionId}' not found in template '{template.Id}'.");
        if (!transition.Enabled) return (null, $"Transition '{transitionId}' is disabled.");
        if (!template.States.TryGetValue(session.CurrentStateId, out var curState)) return (null, $"Current state '{session.CurrentStateId}' missing in template.");
        if (!curState.Enabled) return (null, $"Current state '{curState.Id}' is disabled.");
        if (curState.IsFinal) return (null, $"Session is in final state '{curState.Id}'. No further transitions allowed.");
        if (!transition.FromStates.Contains(curState.Id, StringComparer.OrdinalIgnoreCase)) return (null, $"Transition '{transitionId}' cannot be executed from state '{curState.Id}'.");
        if (!template.States.TryGetValue(transition.ToState, out var toState)) return (null, $"Target state '{transition.ToState}' missing in template.");
        if (!toState.Enabled) return (null, $"Target state '{toState.Id}' is disabled.");

        var hist = new SessionHistoryEntry(DateTimeOffset.UtcNow, transition.Id, curState.Id, toState.Id);
        session.History.Add(hist);
        session.CurrentStateId = toState.Id;
        return (session, null);
    }
} 