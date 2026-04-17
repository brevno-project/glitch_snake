# MCP Unity safe setup for GlitchSnake

Updated: 2026-04-17

## What is already changed in this repo

`Packages/manifest.json` now includes:

- `com.gamelovers.mcp-unity`: `https://github.com/CoderGamester/mcp-unity.git#1.2.0`

Why:

- pinned tag `1.2.0` avoids unexpected updates from `main`;
- Unity installs it as a regular Package Manager dependency.

## Safe default mode

1. Keep the Unity MCP bridge on `localhost` only.
2. Keep `Allow Remote Connections` disabled unless explicitly needed.
3. Review requested tool actions before execution (scene edits, object deletion, menu actions).
4. Commit in small steps before large MCP-driven edits.

## Unity manual steps

1. Open the project in Unity.
2. Wait for package resolve and script compilation after `manifest.json` update.
3. Open `Tools > MCP Unity > Server Window`.
4. Click `Force Install Server` once (if Node server is not installed/built yet).
5. Copy the generated JSON from the `MCP Configuration` block (it contains the exact local path to `Server~/build/index.js`).
6. Verify `Allow Remote Connections` is disabled.
7. Click `Start Server`.
8. Confirm connection in the MCP Unity window status indicator.

## Codex CLI config example (manual fallback)

File: `~/.codex/config.toml`

```toml
[mcp_servers.mcp-unity]
command = "node"
args = ["ABSOLUTE/PATH/TO/mcp-unity/Server~/build/index.js"]
```

In this package version, there is no dedicated `Configure Codex CLI` button. Use manual `~/.codex/config.toml` setup.

## Quick validation flow

1. Ask: `Show available Unity MCP tools`.
2. Run a read-only check: `Read current scene hierarchy`.
3. Run a safe write check in a test scene: `Create Empty GameObject named MCP_Test`.

## Sources

- https://github.com/CoderGamester/mcp-unity
- https://github.com/CoderGamester/mcp-unity/releases
- https://modelcontextprotocol.io/specification/2025-06-18/basic/security_best_practices
- https://modelcontextprotocol.io/specification/2025-11-25/basic/transports
