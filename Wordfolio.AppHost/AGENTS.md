# AppHost Design Rules

**Scope:** `Wordfolio.AppHost` project only.

These rules govern the .NET Aspire orchestration host: launch profiles, port configuration, and resource wiring. They complement but do not replace the project-wide conventions in the root `AGENTS.md`.

---

## Launch Profiles

Two launch profiles are defined in `Properties/launchSettings.json`:

| Profile | Behavior |
|---------|----------|
| `https` | Default profile. Aspire assigns random ports to the API and frontend on each run. |
| `https-fixed-frontend` | Sets `WORDFOLIO_FIXED_FRONTEND_PORT=true`, which pins the frontend to the port specified in `FixedFrontendPortOptions.FrontendPort` (defined in `appsettings.Development.json`). The API still gets a random port. |

Use the fixed-frontend profile when external tools (e.g. a browser extension or mobile app) need a stable frontend URL:

```bash
dotnet run --launch-profile https-fixed-frontend
```

## Port Configuration Rules

- The frontend port is only fixed when `WORDFOLIO_FIXED_FRONTEND_PORT` is `true`. The default profile always uses random ports.
- The API port is never fixed. External access goes through the frontend; the API is not exposed directly.
- The fixed frontend port value is configured in `appsettings.Development.json` under the `FixedFrontendPortOptions` section and bound to the `FixedFrontendPortOptions` record in `Configuration.cs`.
- The database port is configured in `appsettings.Development.json` under the `DatabaseOptions` section and is always fixed (not affected by the launch profile).

## Resource Wiring Rules

- Resources are wired in dependency order: PostgreSQL, then migration runner, then API, then frontend.
- The migration runner runs to completion (`WaitForCompletion`) before the API starts.
- The frontend references the API (`WithReference`), which injects the API's URL automatically via Aspire service discovery.
- Secrets (database credentials, API keys) are declared as parameters, not hardcoded.
