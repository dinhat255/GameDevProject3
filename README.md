# HauntLoop: Endless Waves

## Overview

- Unity 6.0.2f1 project that prototypes a Halloween-themed 2D survival arena with melee combat, EXP progression, and scalable enemy waves.
- Designed for tight gameplay loops, responsive UI, atmospheric audio, and performance-safe VFX using pooling.

## Play & Demo

[https://dinhat255.itch.io/hauntloopendlesswaves](https://dinhat255.itch.io/hauntloopendlesswaves)

## Highlights

- **Core gameplay loop**: player movement, sword melee combat, **enemy AI**, EXP pickups, and progressive wave scaling.
- **Performance optimization**: **object pooling** for enemies, EXP gems, and VFX to reduce allocations and avoid GC spikes during intense combat.
- **UI & audio systems**: HUD, **leaderboard**, tutorial overlay, pause/menu, FPS counter, and an **Audio Manager** with state-based BGM/SFX transitions.
- **Scalable architecture**: structured scenes, **prefabs**, **animation controllers**, and assets for modular Halloween content.
- **Version control**: **Git** workflow (feature → develop → main) with clean commit history.

## Tech stack

- Unity 6.0.2f1 (URP, Tilemap, physics-driven collisions)
- C# + Unity Input System + TextMesh Pro
- Git workflow (feature → develop → main)
