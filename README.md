Bus Jam - High Performance Puzzle Mechanics Template
A robust, production-ready puzzle game template for Unity, focused on high-performance rendering, clean C# architecture (SOLID), and professional game feel.

🚀 Technical Highlights
High-End Performance: Optimized to run at 500+ FPS on desktop with approximately 50 Draw Calls (Batches) using Material Property Blocks to prevent breaking batching while allowing unique per-object properties.

Clean Architecture (SOLID): Strict adherence to the Single Responsibility Principle (SRP). Game logic is decoupled into specialized managers (GridManager, BusManager, SoundManager, InteractionManager).

SDF-Based Shader Grid: A custom-written shader handles the background grid, using Signed Distance Fields (SDF) for dynamic rounded corners, procedural borders, and masking, all while managing tile data via global arrays.

Advanced Sound System: A ScriptableObject-based sound architecture supporting Crossfading Music, randomized pitching for SFX, and delayed execution via DOTween integration.

Modular Level Data: Levels are entirely data-driven using ScriptableObjects, allowing for easy level creation without touching the scene hierarchy.

🛠 Tech Stack
Unity 2022.3+ (Built-in Render Pipeline / URP Compatible)

C# Scripting: Advanced event-driven logic and Singleton-based service registration.

DOTween (Demigiant): Orchestrates all procedural animations, UI transitions, and movement sequences.

ShaderLab / HLSL: Custom environment shader for optimized grid rendering.

ScriptableObjects: Used for Level Data, Color Catalogs, and Sound Databases.

🎮 Getting Started
Play the Game
The Official Flow: Open the MenuScene and press Play. This ensures all persistent managers (like SoundManager) are initialized correctly.

Developer Quick-Start: You can also start directly from the GameScene. The GameManager is designed to handle local testing by loading the testLevelOverride if assigned in the Inspector.

🛠 Level Editor Guide (Detailed)
The project includes a custom-built Level Editor to streamline content creation.

1. Opening the Editor
Open the LevelEditor scene found in Assets/Scenes/Editor/.

2. Editor Setup
Locate the LevelEditorContext object in the Hierarchy.

In the Inspector, you can either:

Load Existing: Drag an existing LevelDataSO from Assets/Resources/Levels/ into the "Level Data" slot.

Create New: Set the desired Width/Height and click "Create New Level SO".

3. Using the Brushes (Painting)
The editor uses a "Brush System" to paint the grid in the Scene View:

Passenger Brushes: Select a color (Red, Blue, Green, etc.) and click on any tile to place a passenger.

Obstacle Brush: Marks a tile as a wall/blocker.

Empty Floor: Reverts a tile to a standard playable floor.

Eraser Brushes: Removes objects or deletes tiles from the active grid layout.

4. Configuring Level Mechanics
Grid Size: Adjust Width and Height in real-time. The grid will automatically resize and maintain existing data where possible.

Bus Spawn Sequence: This is the most critical part. Define the order in which buses arrive (e.g., Purple -> Blue -> Red).

Level Timer: Set the total duration allowed for the level in seconds.

5. Validation & Publishing
The Editor includes an Auto-Validation System:

Rule of 3: The system checks if the number of passengers for each color is a multiple of 3.

Bus Match: It ensures the Bus Spawn Sequence contains enough capacity for all placed passengers.

Publish: Once the "Level Valid" info box appears, click "PUBLISH / SAVE LEVEL". This will save the ScriptableObject and rename it according to your "Level Name" input.

🏗 System Architecture
GameContext: A central service locator for decoupled communication between managers.

BusManager: Handles the mathematical queueing of buses and the 3D movement logic for the bus sequence.

InteractionManager: Manages raycasting, input validation, and movement path execution.

SoundManager: Controls the global audio state with professional Crossfade transitions between Menu and Gameplay.