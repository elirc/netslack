# Command Cheatsheet

Status key: **verified** means run here; **failed here** means attempted and failed in this environment; **inferred** means supported by files but not run.

| Purpose | Command | Status | Evidence |
| --- | --- | --- | --- |
| Inventory files | `rg --files c:\Users\Owner\Desktop\.netslack` | verified | Used during doc exploration. |
| Scoped git status | `git -C c:\Users\Owner status --short -- Desktop/.netslack` | verified | Returned `?? Desktop/.netslack/`. |
| .NET SDK version | `dotnet --version` | failed here | `dotnet` not recognized on PATH. |
| Restore solution | `dotnet restore .\NetSlack.sln` | inferred | Solution projects in `NetSlack.sln:5-14`. |
| Test solution | `dotnet test .\NetSlack.sln` | inferred | Tests in `tests/NetSlack.Application.Tests/WorkflowTests.cs:15-63`. |
| Run API | `dotnet run --project .\src\NetSlack.Api\NetSlack.Api.csproj` | inferred | Launch URLs in `src/NetSlack.Api/Properties/launchSettings.json:4-12`. |
| Client install | `cd client; npm install` | not run by request | Packages in `client/package.json:12-23`. |
| Client dev | `cd client; npm run dev` | inferred | Script in `client/package.json:7`; Vite port in `client/vite.config.ts:6-9`. |
| Client build | `cd client; npm run build` | skipped here | No `client/node_modules`; build script in `client/package.json:8`. |
| Client lint | `cd client; npm run lint` | inferred/risk | Script in `client/package.json:10`; no ESLint deps in `client/package.json:18-23`. |
| Docker compose | `docker compose -f .\deploy\docker-compose.yml up --build` | inferred | Services in `deploy/docker-compose.yml:1-29`. |
| API health | `Invoke-WebRequest http://localhost:5208/health` | inferred | Health endpoint mapped in `src/NetSlack.Api/Program.cs:75`. |
| Swagger | `https://localhost:7208/swagger` | inferred | Swagger enabled in dev in `src/NetSlack.Api/Program.cs:61-64`. |
| EF migrations | `dotnet ef migrations add InitialCreate --project .\src\NetSlack.Infrastructure --startup-project .\src\NetSlack.Api` | future | No migrations found; EF packages in `src/NetSlack.Infrastructure/NetSlack.Infrastructure.csproj:8-12`. |

