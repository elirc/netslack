# Domain Glossary

| Term | Meaning in this repo | Evidence | Drill |
| --- | --- | --- | --- |
| Workspace | Tenant-like collaboration container with name, slug, members | `src/NetSlack.Domain/Workspaces/Workspace.cs:23-39` | Explain why owner membership is created inside `Workspace.Create`. |
| Workspace member | User joined to workspace with role | `src/NetSlack.Domain/Workspaces/WorkspaceMember.cs:11-20` | Identify where membership is checked before listing channels. |
| Workspace role | Owner/Admin/Member/Guest enum | `src/NetSlack.Domain/Workspaces/WorkspaceRole.cs:1-9` | Propose where role-based permissions would live. |
| Channel | Conversation container inside workspace | `src/NetSlack.Domain/Channels/Channel.cs:26-38` | Trace how channel name becomes normalized. |
| Channel type | Public, Private, DirectMessage | `src/NetSlack.Domain/Channels/ChannelType.cs:1-8` | Describe how public and private read access differs. |
| Channel member | User joined to channel | `src/NetSlack.Domain/Channels/ChannelMember.cs:11-18` | Explain why direct messages need stricter rules than public channels. |
| Message | Authored body in a channel with posted/edited timestamps | `src/NetSlack.Domain/Messages/Message.cs:21-29` | Find where body length is enforced. |
| Message author | DTO projection of user data on a message | `src/NetSlack.Application/Messages/MessageDto.cs:3-11` | Explain why the client should not need another user lookup. |
| Current user | Application-facing identity abstraction | `src/NetSlack.Application/Common/ICurrentUser.cs:1-11` | Map claims to this interface. |
| Domain event | In-memory event collection on entities | `src/NetSlack.Domain/Common/Entity.cs:18-22` | Investigate: no dispatcher is wired yet. What would be needed? |

## Transferable Concepts

- Invariant: a rule that should always hold, like non-empty message bodies in `src/NetSlack.Domain/Messages/Message.cs:31-38`.
- Boundary: a line where ownership changes, like API contracts in `src/NetSlack.Api/Contracts/MessageContracts.cs:1-4` versus application commands in `src/NetSlack.Application/Messages/MessageDto.cs:13`.
- Authorization: deciding whether an authenticated principal may access a resource, as in `src/NetSlack.Application/Messages/MessageService.cs:53-114`.
- Consistency: saving workspace and default channel together in one EF `SaveChangesAsync` call in `src/NetSlack.Application/Workspaces/WorkspaceService.cs:33-38`.

