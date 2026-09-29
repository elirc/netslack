# Trace lab: follow four requests

Use the following trace table as a lab notebook. Request A is authenticated and asks for `Same Slug`; request B may already have committed the normalized `same-slug`.

| Event | A existing | A concurrent | A unrelated error | A success |
|---|---:|---:|---:|---:|
| authenticated | yes | yes | yes | yes |
| normalized value | same-slug | same-slug | same-slug | same-slug |
| precheck | row found | no row | no row | no row |
| entities added | zero | workspace + general | workspace + general | workspace + general |
| save | no | exception | exception | one commit |
| recovery query | none | B row found | no row | none |
| public result | conflict | conflict | original error | DTO |

Now alter one fact at a time. If A is unauthenticated, the trace ends before normalization and no existence information is returned. If B committed a different slug, recovery finds no matching row and A’s original database exception remains. If the caller supplies whitespace and mixed case, the data boundary still points to `same-slug`. If the general channel addition fails after workspace insertion in a provider transaction that is not atomic, this bounded change does not repair partial commits; that requires transaction configuration and a separate test.

Run the lab against [the actual service](../src/NetSlack.Application/Workspaces/WorkspaceService.cs) and compare with the explicit fake state transitions in [Program.cs](../tests/NetSlack.ScopeHarness/Program.cs). Ask whether each observed row is persisted or merely tracked. That question is the central debugging skill. The original integration test context is linked at [WorkflowTests.cs](../tests/NetSlack.Application.Tests/WorkflowTests.cs); its snapshot documents the starting behavior.
