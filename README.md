# 2D Game Engine (C# / Raylib)

Created by Adithya Kannan
---
A modular retro-style 2D game engine built with **C# (.NET 8)** and **Raylib-cs**, plus two sample games.


## Solution layout

```text
RetroGames.sln
├── MyEngine/        Reusable engine class library
├── FlappyBird/      Sample game
└── ShootTheDuck/    Sample game (bonuses, coins, multipliers)
```

### Engine systems (`MyEngine`)

- Fixed-timestep game loop
- Scene / entity management
- Input (keyboard + mouse)
- Texture rendering + sprite sheets
- AABB / point collision
- Audio (SFX + music streaming)
- Asset caching

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Windows (Raylib native binaries via NuGet)

## Build

```bash
dotnet build RetroGames.sln
```

## Run samples

```bash
dotnet run --project FlappyBird
dotnet run --project ShootTheDuck
```

### Flappy Bird controls

- **Space** / **Left click** — flap
- Same input restarts after game over

### Shoot the Duck controls

- **Mouse** — aim (custom crosshair)
- **Left click** — shoot / collect coins & bonuses
- **R** — reload
- **Space** / **Click** — start from intro / return to title

