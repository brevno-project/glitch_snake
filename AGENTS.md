# GlitchSnake — project instructions for Codex

## Project goal
Build a small Unity 2D game called GlitchSnake.
It is based on classic Snake, but includes temporary glitch mechanics.

## Current MVP scope
Implement only the MVP first:
- Main Menu scene
- Game scene
- Snake movement on a grid
- Food spawning
- Snake growth
- Score
- Game Over
- Restart

## Glitch mechanics for later
Do not implement all of them immediately.
Add them only after the base Snake is fully playable.
Planned glitch mechanics:
- reverse controls
- fake food
- temporary speed change

## Tech stack
- Unity 6.3 LTS
- Unity 2D Core project
- C#
- VS Code
- Git

## Important rules
- Keep the architecture simple and beginner-friendly.
- Do not overengineer.
- Keep scripts short and readable.
- Prefer clear Unity patterns over advanced abstractions.
- Explain non-obvious changes.
- Do not add extra systems outside MVP unless explicitly requested.
- Before major implementation, first propose a plan.

## Repo structure
Important folders:
- Assets/
- Packages/
- ProjectSettings/

Preferred folders inside Assets:
- Assets/Scenes
- Assets/Scripts
- Assets/Prefabs
- Assets/Sprites
- Assets/UI

## Done means
A task is done only when:
- the requested files are created or updated
- the changes match the requested scope
- the result is explained clearly
- manual Unity steps are listed if needed