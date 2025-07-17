namespace Infonetica.Workflow.Core;

public sealed class WorkflowTemplate
{
    public string Id { get; init; }
    public string Name { get; init; }
    public IReadOnlyDictionary<string, State> States { get; init; }
    public IReadOnlyDictionary<string, Transition> Transitions { get; init; }
    public string InitialStateId { get; init; }

    public WorkflowTemplate(string id, string name,
        IEnumerable<State> states,
        IEnumerable<Transition> transitions)
    {
        Id = id;
        Name = name;
        var stateDict = states.ToDictionary(s => s.Id, StringComparer.OrdinalIgnoreCase);
        States = stateDict;
        var transitionDict = transitions.ToDictionary(a => a.Id, StringComparer.OrdinalIgnoreCase);
        Transitions = transitionDict;
        InitialStateId = stateDict.Values.Single(s => s.IsInitial).Id;
    }
} 