# Infonetica-Project

---

## Environment

- Requires: .NET 8 SDK or later
- Tested on: Windows 11

---

## Quick Start

1. Clone this repo and open the `infonetica-project` folder.
2. Build & run:

   ```sh
   dotnet run
   ```

   The API listens on http://localhost:5000 (HTTP) & https://localhost:7000 (HTTPS) by default.

---

## Project Structure

```
infonetica-project/
 ├─ Infonetica.Workflow.csproj
 ├─ Startup.cs
 ├─ Core/
 │   ├─ State.cs
 │   ├─ Transition.cs
 │   ├─ WorkflowTemplate.cs
 │   ├─ WorkflowSession.cs
 │   └─ SessionHistoryEntry.cs
 ├─ Models/
 │   ├─ CreateTemplateRequest.cs
 │   ├─ TemplateDto.cs
 │   ├─ SessionDtos.cs
 │   └─ ApiErrorResponse.cs
 ├─ Logic/
 │   └─ WorkflowManager.cs
 ├─ Storage/
 │   └─ JsonFileStore.cs
 └─ README.md
```

---

## Minimal API Surface

Templates

* POST   /workflow-templates – create new template (states + transitions in one request).
* GET    /workflow-templates – list templates.
* GET    /workflow-templates/{id} – get template.

Sessions

* POST   /workflow-sessions – start new session from template.
* GET    /workflow-sessions – list sessions.
* GET    /workflow-sessions/{id} – get session (current state + history).
* POST   /workflow-sessions/{id}/actions/{transitionId} – execute transition on session.

Persistence (optional)

* POST   /_admin/export – returns JSON snapshot of templates + sessions.
* POST   /_admin/import – replace in‑memory store from posted snapshot JSON.

---

## Example Workflow (Step-by-Step)

### 1. **Start the API**

Open PowerShell and run:
```powershell
dotnet run --project infonetica-project/Infonetica.Workflow.csproj
```
Leave this window open.

---

### 2. **Create a Workflow Template**

Open a new PowerShell window and run:
```powershell
$body = '{ "id":"docflow", "name":"Document Review", "states":[{"id":"draft","name":"Draft","isInitial":true},{"id":"review","name":"In Review"},{"id":"approved","name":"Approved","isFinal":true},{"id":"rejected","name":"Rejected","isFinal":true}], "transitions":[{"id":"submit","name":"Submit for review","fromStates":["draft"],"toState":"review"},{"id":"approve","name":"Approve","fromStates":["review"],"toState":"approved"},{"id":"reject","name":"Reject","fromStates":["review"],"toState":"rejected"},{"id":"revise","name":"Send back to draft","fromStates":["review"],"toState":"draft"}] }'
Invoke-WebRequest -Uri "http://localhost:5000/workflow-templates" -Method POST -Body $body -ContentType "application/json"
```
You should see a JSON response with your template.

---

### 3. **Start a Workflow Session**

```powershell
$body = '{"templateId":"docflow"}'
Invoke-WebRequest -Uri "http://localhost:5000/workflow-sessions" -Method POST -Body $body -ContentType "application/json"
```
Copy the `id` from the response (let's call it `<YOUR_SESSION_ID>`).

---

### 4. **Move to Next State ("Submit for review")**

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/workflow-sessions/<YOUR_SESSION_ID>/actions/submit" -Method POST
```
Replace `<YOUR_SESSION_ID>` with the actual id you copied above. The `currentState` should now be `review`.

---

### 5. **Approve the Document**

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/workflow-sessions/<YOUR_SESSION_ID>/actions/approve" -Method POST
```
The `currentState` should now be `approved`, and `isFinal` should be `true`.

---

### 6. **Inspect the Session**

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/workflow-sessions/<YOUR_SESSION_ID>" -Method GET | Select-Object -ExpandProperty Content
```
You’ll see a JSON object with the full history of actions.

---

### 7. **Try an Invalid Action (Should Fail)**

```powershell
Invoke-WebRequest -Uri "http://localhost:5000/workflow-sessions/<YOUR_SESSION_ID>/actions/approve" -Method POST
```
You should get an error message saying you can’t act on a final state.

---

## Assumptions & Shortcuts

- Templates are created in a single POST (not incrementally).
- IDs can be supplied or auto-generated.
- In-memory storage; use export/import for persistence.
- No authentication or user management.
- No pagination or filtering on list endpoints.
- No concurrency control beyond atomic dictionary operations.
- History is not bounded or paginated.

---

## Known Limitations

- No PATCH/PUT endpoints for incremental template editing (all states/transitions must be provided at creation).
- Data is lost on server restart unless exported/imported.
- No user/audit tracking.
- No OpenAPI/Swagger UI.
- No pagination or filtering for large lists.
- No advanced graph validation (e.g., unreachable states, dead transitions).

---

## Extensibility Notes (TODO markers you might add later)

* PATCH endpoints to add/disable states or transitions.
* Graph validation (unreachable states, dead transitions).
* Bulk history export, audit user info.
* Pluggable persistence (database, redis, etc.) behind interface.
* Swagger/OpenAPI generation.

---

## License

MIT License

Copyright (c) 2025 Vishesh Gupta

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
