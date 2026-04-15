# Agent Platform Structure (Master Agent + MCP + Tools)

Last updated: 2026-04-14

## 1) Goal

Create one robust, distributable agent platform that:

- Knows all internal tools and MCP servers.
- Routes tasks to correct capability.
- Enforces standards and safety.
- Avoids duplicated tooling/projects.

Critical constraint:

- `BridgeDesignAutomation` remains sacred/no-touch.

---

## 2) Design Principle: Keep Standards Separate from Scaffolder

Reason:

- **Standards** define rules (what good looks like).
- **Scaffolder** generates artifacts (how new work starts).

If coupled, standard updates force generator rewrites. If separated, scaffolder reads standards version and stays stable.

---

## 3) Recommended Folder Layout

```text
AgentPlatform/
  README.md
  AGENT_PLATFORM_STRUCTURE.md
  AGENT_MANIFEST.md

  standards/
    README.md
    engineering/
      coding-standards.md
      testing-standards.md
      security-standards.md
      naming-conventions.md
    design/
      dar-ui-standards.md
      ux-guidelines.md
    architecture/
      adr-template.md
      decision-log/

  scaffolder/
    README.md
    templates/
      mcp-service/
      cli-tool/
      dotnet-plugin/
      python-pipeline/
    prompts/
    schema/

  mcps/
    README.md
    MCP_INDEX.md
    azdo-mcp/
    dll-inspector-mcp/
    helpfile-mcp/

  tools/
    README.md
    vs-tool/
      README.md
      vs
      schema/
    wrappers/

  agents/
    README.md
    master-agent.md
    routing-policy.md
    subagents/
      build-agent.md
      mcp-agent.md
      scaffold-agent.md
      standards-agent.md

  config/
    mcp-registry.json
    tool-registry.json
    agent-config.yaml

  docs/
    how-to/
    reference/
    explanation/
    tutorials/

  evals/
    scenarios/
    regression/
    prompt-cases/

  scripts/
    bootstrap.sh
    bootstrap.ps1
    healthcheck.sh
    package-release.sh
```

---

## 4) Core Components

## 4.1 Master Agent

Files:

- `agents/master-agent.md`
- `agents/routing-policy.md`
- `AGENT_MANIFEST.md`

Responsibilities:

- Parse task intent.
- Route to tool/MCP/subagent.
- Enforce approval gates for risky actions.
- Record decisions and outcomes.

## 4.2 MCP Layer

Files:

- `mcps/MCP_INDEX.md`
- `config/mcp-registry.json`

Responsibilities:

- Register all MCP servers with path, start command, env vars, health endpoint/check command.
- Declare capability tags (e.g., `azure-devops`, `dll-inspection`, `help-docs`).
- Expose stable contracts to master agent.

## 4.3 Tool Layer

Files:

- `tools/vs-tool/vs`
- `config/tool-registry.json`

Responsibilities:

- Provide stable CLI actions for build/test/restore/clean.
- Replace shell-only aliases/functions with documented, versioned tools.

## 4.4 Standards Layer

Files:

- `standards/**/*`

Responsibilities:

- Define coding, design, security, naming, architecture decision standards.
- Version standards independently from scaffolder and tools.

## 4.5 Scaffolder Layer

Files:

- `scaffolder/templates/**/*`
- `scaffolder/schema/*`

Responsibilities:

- Generate new MCP/tool/project skeletons.
- Consume standards version + project type + policy constraints.

---

## 5) Registries and Contracts

Minimum contract files:

- `config/mcp-registry.json`
  - id, name, path, start command, env keys, health check, capabilities, owner
- `config/tool-registry.json`
  - id, command, args schema, output format, safety level
- `AGENT_MANIFEST.md`
  - what master agent can do, cannot do, and required approvals

Use JSON Schema for validation of registry files.

---

## 6) Safety and Governance Model

Rules:

1. No destructive filesystem or git action without explicit approval.
2. No hidden side effects; all tool commands logged.
3. Secrets only via env/config vault, never hardcoded.
4. High-risk actions require confirmation block.
5. Unknown tool result => fail safe, ask user.

Add:

- `scripts/healthcheck.sh` to verify MCPs/tools before use.
- Eval gate: regressions must pass before release.

---

## 7) Documentation Standard

Use Diátaxis split:

- `docs/tutorials` (learn by doing)
- `docs/how-to` (task completion)
- `docs/reference` (flags, schemas, APIs)
- `docs/explanation` (architecture decisions, tradeoffs)

Each MCP/tool gets:

1. Purpose
2. Inputs/outputs
3. Commands
4. Examples
5. Failure modes + troubleshooting

---

## 8) Migration Plan from Current State

Phase 1 — Registry and docs first

- Create `mcp-registry.json` for:
  - `azdo-mcp`
  - `dll-inspector-mcp`
  - `HelpFileMCP`
- Create `MCP_INDEX.md` with run instructions.

Phase 2 — vs tool hardening

- Extract current bashrc `vs` behavior.
- Implement standalone script with `--help` and subcommands.
- Add docs + schema + tests.

Phase 3 — standards and scaffolder split

- Move/duplicate design standards into `standards/design`.
- Keep scaffolder templates in `scaffolder/templates`.
- Add standards version field consumed by scaffolder.

Phase 4 — master agent routing

- Implement routing policy file.
- Add capability tags and fallback rules.

Phase 5 — packaging

- Add bootstrap and release script.
- Produce distributable bundle.

---

## 9) MCP Path References (Current)

- Azure DevOps MCP: `/mnt/c/Users/jjhaddad/Documents/Work/azdo-mcp`
- DLL Inspector MCP: `/mnt/c/Users/jjhaddad/Documents/Work/zeroTouch/dll-inspector-mcp`
- HelpFileMCP placeholder: `/mnt/c/Users/jjhaddad/Documents/Work/HelpFileMCP`

---

## 10) Definition of Done for “Robust + Distributable”

Done when:

- All MCPs registered with health checks.
- `vs` is standalone tool, not shell-only function.
- Master agent has explicit routing and safety policy.
- Standards and scaffolder separated and versioned.
- Evals/regression suite exists and runs in CI.
- One bootstrap command sets up working environment.
