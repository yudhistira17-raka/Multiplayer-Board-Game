# Multiplayer Board Game — Unity \& C#

> A real-time multiplayer board game developed in Unity as a Bachelor's thesis project at Universitas Sumatera Utara. Built with a custom TCP networking layer, turn-based game logic, and OOP architecture.

\---

## Overview

This project is a digital board game where two players compete in real time over a local network. Each player rolls a dice and moves their token along a path of waypoints. The first player to reach the end wins. The game is built entirely in Unity using C# with a focus on clean object-oriented design and networked state synchronization.

\---

## Features

* Real-time multiplayer over TCP/IP (local network)
* Turn-based dice roll and player movement system
* Animated dice with random outcome generation
* Smooth waypoint-based player movement
* Server-side message sequencing with client-side sync recovery
* Win condition detection and game-over state management
* Scene management: main menu → game → result

\---

## Tech Stack

|Layer|Technology|
|-|-|
|Engine|Unity (2D)|
|Language|C#|
|Networking|TCP/IP via `System.Net.Sockets`|
|Architecture|Object-Oriented Programming (OOP)|
|Version Control|Git|

\---

## Architecture \& OOP Design

The project is structured around five MonoBehaviour-derived classes, each with a single, clearly defined responsibility.

```
MonoBehaviour (Unity Engine)
    ├── NetworkScript     — TCP connection, background thread, message queue
    ├── Dice              — Dice roll animation, send move to server
    ├── GameControl       — Game state, turn logic, win condition
    ├── FollowThePath     — Waypoint-based player movement
    └── Menu              — Player selection, scene transition
```

### Encapsulation

Each class exposes only what other components need to access, and hides everything else.

```csharp
// NetworkScript.cs
private string IP = "localhost";         // internal config, not exposed
private int port = 54000;

public static Queue<string> messageQueue; // intentionally public — GameControl reads this
public static TcpClient client;           // shared by SendMessageToServer
```

```csharp
// FollowThePath.cs
\[SerializeField]
private float moveSpeed = 1f;            // visible in Inspector, not accessible by other scripts

\[HideInInspector]
public int waypointIndex = 0;            // accessible by GameControl, hidden from Inspector
```

### Single Responsibility Principle

Each class owns exactly one domain:

* `NetworkScript` — handles all TCP socket operations and feeds messages into a thread-safe queue
* `Dice` — runs the roll coroutine and sends the result to the server; knows nothing about movement
* `FollowThePath` — moves a player along waypoints when `moveAllowed = true`; knows nothing about rules
* `GameControl` — the only class that reads game state, controls turns, and detects win conditions
* `Menu` — handles pre-game player selection and scene loading only

### Producer-Consumer Pattern (Networking)

A background thread continuously reads from the TCP stream and enqueues messages. The Unity main thread dequeues and processes them once per frame in `Update()`. This keeps networking off the main thread without blocking rendering.

```csharp
// NetworkScript.cs — producer (background thread)
public static void MessageReader() {
    while (true) {
        stream.Read(bytes, 0, client.ReceiveBufferSize);
        messageQueue.Enqueue(Encoding.UTF8.GetString(bytes));
    }
}

// GameControl.cs — consumer (main thread, called every frame)
void Update() {
    if (NetworkScript.messageQueue.Count > 0) {
        string message = NetworkScript.messageQueue.Dequeue();
        // parse and act on the message
    }
}
```

### Message Sequencing \& Sync Recovery

The server tags each outbound message with a sequence number. The client tracks the expected sequence counter (`inboundMessageCounter`) and drops out-of-order messages. If a mismatch is detected, the client sends a sync request to ask the server to resend from the last acknowledged message.

```csharp
// GameControl.cs
if (messageIndex == inboundMessageCounter) {
    MovePlayer(player);
    inboundMessageCounter++;
} else if (!isSyncing) {
    netObject.SendMessageToServer("s" + inboundMessageCounter + "x");
    isSyncing = true;
}
```

### Static Shared State

`GameControl` exposes shared game state as static members so other classes can read them without holding a direct reference to the `GameControl` instance. This is a common Unity pattern for global game state.

```csharp
public static bool gameOver = false;
public static int diceSideThrown = 0;
public static void MovePlayer(int playerToMove) { ... }
public static void SendMovePlayerToServer(int player, int amount) { ... }
```

\---

## Project Structure

```
Assets/
├── Scripts/
│   ├── NetworkScript.cs     — TCP client, background reader thread, message queue
│   ├── Dice.cs              — Dice roll coroutine, sends result to server
│   ├── FollowThePath.cs     — Waypoint movement for player tokens
│   ├── GameControl.cs       — Game state, turn management, win detection
│   └── Menu.cs              — Pre-game player selection
├── Resources/
│   └── DiceSides/           — Dice face sprites (loaded at runtime via Resources.LoadAll)
└── Scenes/
    ├── Menu                 — Player selection screen
    └── Game                 — Main game scene
```

\---

## How to Run

1. Clone this repository
2. Open the project in Unity (2020.3 LTS or later recommended)
3. Start the TCP server on `localhost:54000`
4. Open two instances of the game (or build and run on two machines on the same network)
5. Player 1 clicks **Player 1**, Player 2 clicks **Player 2**
6. Click the dice to roll — turns alternate automatically

\---

## What I Learned

* Designing a multi-class OOP architecture in Unity with clear separation of concerns
* Managing cross-thread communication between Unity's main thread and a background network reader
* Implementing TCP socket communication
* Building a message sequencing and sync recovery protocol from scratch
* Structuring a full game loop: menu → gameplay → win state → restart

\---

## Author

**Mhd. Raka Prayudhistira**
Bachelor of Computer Science — Universitas Sumatera Utara

