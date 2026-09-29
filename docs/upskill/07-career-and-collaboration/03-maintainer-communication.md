# Maintainer Communication

## Ask Without Outsourcing Thinking

Weak: "How do I add auth?"

Better: "I traced dev-token auth at `src/NetSlack.Api/Endpoints/AuthEndpoints.cs:15-40` and current user mapping at `src/NetSlack.Api/Middleware/CurrentUser.cs:8-21`. I think OIDC can replace token issuance while preserving `ICurrentUser`. Is that direction aligned?"

## Bug Report Template

```md
## Symptom
## Reproduction
## Expected
## Actual
## Evidence
- Code anchor:
- Logs/trace id:
## Suspected layer
## Minimal test idea
```

## Responding To Review

- Thank the reviewer.
- Restate the issue.
- Push a focused fix or explain tradeoff with evidence.
- Avoid drive-by refactors.

Example: "Good catch on resource auth. I added a service-level test modeled after `WorkflowTests.cs:43-63` and kept the endpoint thin like `MessageEndpoints.cs:26-35`."

## Respectful Disagreement

Use evidence: "I agree the UI should hide private channels, but I think the blocking check belongs in Application because `MessageService.cs:70-76` already protects direct API calls."

