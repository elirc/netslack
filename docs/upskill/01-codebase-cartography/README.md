# Codebase Cartography

Cartography is the discipline of building a map before making changes. In this repo, start with project ownership, then follow data across UI, API, application services, domain entities, EF mappings, and tests.

Read in this order:

1. `01-system-map.md`
2. `02-file-reading-order.md`
3. `03-domain-glossary.md`
4. `04-runtime-and-tooling-map.md`
5. `05-key-flows.md`

Checkpoint: before editing code, you should be able to point to the line where a request becomes an application command, where authorization is enforced, where persistence is configured, and where the React client stores selected channel state.

