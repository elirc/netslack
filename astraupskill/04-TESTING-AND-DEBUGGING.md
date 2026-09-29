# Testing and debugging the write path

Debug this flow by recording state transitions rather than only the final exception. Log or inspect the normalized slug, whether the precheck ran, how many entities were added, whether save was attempted, and what the post-failure query observed. Never use a broad “unique” message assertion as the primary diagnosis. First reproduce the ordinary duplicate, then force a concurrent winner, then force a failure with no persisted winner. The three outcomes should be conflict before save, conflict after save, and original persistence failure.

The staged harness [Program.cs](../tests/NetSlack.ScopeHarness/Program.cs) uses explicit contracts in [Contracts.cs](../tests/NetSlack.ScopeHarness/Contracts.cs). Its fake database removes pending rows when save fails and inserts a separate workspace for the concurrent case. That matters: a naive fake that leaves the just-added object in the queryable set would make the recovery query pass for the wrong reason. The harness runs the real `WorkspaceService`, `Workspace`, and `Channel` code through source links in [its project file](../tests/NetSlack.ScopeHarness/NetSlack.ScopeHarness.csproj).

A debugging trace can be summarized as follows:

| Step | Existing slug | Concurrent winner | Unrelated failure |
|---|---|---|---|
| precheck | present | absent | absent |
| save | skipped | throws | throws |
| post-query | not reached | present | absent |
| public result | conflict | conflict | original error |

If a test says conflict for an outage, inspect whether the fake accidentally queries local pending entities. If success lacks the general channel, inspect additions before changing exception handling. Native .NET8 and provider execution remain separate checks documented in [VERIFICATION.md](VERIFICATION.md).
