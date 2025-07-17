using Infonetica.Workflow.Core;
using Infonetica.Workflow.Models;
using Infonetica.Workflow.Storage;
using Infonetica.Workflow.Logic;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<WorkflowManager>();
var app = builder.Build();

// ---- Templates ----
app.MapPost("/workflow-templates", (CreateTemplateRequest req, WorkflowManager mgr) =>
{
    var states = req.States ?? new();
    var transitions = (req.Transitions ?? new()).Select(t => new Transition(t.Id, t.Name, t.FromStates, t.ToState, t.Enabled));
    var (template, err) = mgr.CreateTemplate(req.Id, req.Name, states, transitions);
    return template is not null
        ? Results.Created($"/workflow-templates/{template.Id}", ToDto(template))
        : Results.BadRequest(new ApiErrorResponse("invalid_template", err ?? "error"));
});

app.MapGet("/workflow-templates", (WorkflowManager mgr) =>
{
    var list = mgr.ListTemplates().Select(ToDto);
    return Results.Ok(list);
});

app.MapGet("/workflow-templates/{id}", (string id, WorkflowManager mgr) =>
{
    return mgr.TryGetTemplate(id, out var template)
        ? Results.Ok(ToDto(template))
        : Results.NotFound(new ApiErrorResponse("not_found", $"Template '{id}' not found."));
});

// ---- Sessions ----
app.MapPost("/workflow-sessions", (StartSessionRequest req, WorkflowManager mgr) =>
{
    var (session, err) = mgr.StartSession(req.TemplateId, req.Id);
    if (session is null) return Results.BadRequest(new ApiErrorResponse("start_failed", err ?? "error"));
    return Results.Created($"/workflow-sessions/{session.Id}", ToSessionDto(session, mgr));
});

app.MapGet("/workflow-sessions", (WorkflowManager mgr) =>
{
    var list = mgr.ListSessions().Select(s => ToSessionDto(s, mgr));
    return Results.Ok(list);
});

app.MapGet("/workflow-sessions/{id}", (string id, WorkflowManager mgr) =>
{
    if (!mgr.TryGetSession(id, out var session))
        return Results.NotFound(new ApiErrorResponse("not_found", $"Session '{id}' not found."));
    return Results.Ok(ToSessionDto(session, mgr));
});

app.MapPost("/workflow-sessions/{id}/actions/{transitionId}", (string id, string transitionId, WorkflowManager mgr) =>
{
    var (session, err) = mgr.ExecuteTransition(id, transitionId);
    if (session is null) return Results.BadRequest(new ApiErrorResponse("execute_failed", err ?? "error"));
    return Results.Ok(ToSessionDto(session, mgr));
});

// ---- Admin / Snapshot ----
app.MapPost("/_admin/export", (WorkflowManager mgr) =>
{
    var json = JsonFileStore.ExportJson(mgr);
    return Results.Text(json, "application/json");
});

app.MapPost("/_admin/import", async (HttpRequest http, WorkflowManager mgr) =>
{
    using var reader = new StreamReader(http.Body);
    var json = await reader.ReadToEndAsync();
    var (ok, err) = JsonFileStore.ImportJson(json, mgr);
    return ok ? Results.Ok() : Results.BadRequest(new ApiErrorResponse("import_failed", err ?? "error"));
});

app.Run();

// ---- local helpers ----
static TemplateDto ToDto(WorkflowTemplate t) => new(
    t.Id,
    t.Name,
    t.States.Values,
    t.Transitions.Values,
    t.InitialStateId
);

static SessionDto ToSessionDto(WorkflowSession s, WorkflowManager mgr)
{
    mgr.TryGetTemplate(s.TemplateId, out var template);
    var cur = s.CurrentStateId;
    var isFinal = template != null && template.States.TryGetValue(cur, out var st) && st.IsFinal;
    return new SessionDto(s.Id, s.TemplateId, cur, isFinal, s.History);
} 