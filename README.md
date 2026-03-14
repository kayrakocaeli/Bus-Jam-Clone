# 🚌 Bus Jam - High Performance Mechanics Template

<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3%2B-blue?style=for-the-badge&logo=unity" alt="Unity Version">

  <img src="https://img.shields.io/badge/C%23-SOLID-green?style=for-the-badge&logo=csharp" alt="C# SOLID">
  <img src="https://img.shields.io/badge/Render%20Pipeline-Built--in%20RP-red?style=for-the-badge" alt="Built-in RP">
</p>

---

## 🌟 Overview

This project was developed as a **Technical Case Study** to demonstrate a production-ready puzzle game core in Unity. It is engineered for maximum performance and architectural scalability, solving common industry challenges like draw-call optimization, modular data management, and decoupled system communication.

---

## 🚀 Key Technical Pillars

### ⚡ 1. High-Performance Rendering (Built-in Optimized)
* **Batching Preservation:** Uses `MaterialPropertyBlock` to pass per-object data (colors, outlines) to the GPU. This prevents breaking **Static/Dynamic Batching**, which is crucial for Built-in RP performance.
* **SDF Grid Shader:** A custom HLSL shader written specifically for Built-in RP, using **Signed Distance Fields (SDF)** for procedural borders and dynamic masking.
* **Efficient Memory:** Zero-allocation runtime for passengers and buses via a pre-warmed `PoolManager`.

### 🏗️ 2. Clean Architecture (SOLID Case Study)
* **Service Locator Pattern:** Decoupled manager communication via a central `GameContext`.
* **Data-Driven Design:** Level configurations are stored in `ScriptableObjects`, making the logic entirely independent of scene hierarchy.
* **SRP Adherence:** * `BusManager`: Handles mathematical queuing and 3D movement logic.
    * `InteractionManager`: Manages input processing and path validation.
    * `GridManager`: Manages spatial mapping and shader property updates.

### 🎵 3. Advanced Audio Engine
* **Crossfade Engine:** Smooth volume transitions between Menu and Gameplay music using DOTween.
* **Dynamic Pitching:** Randomized SFX pitching to prevent repetitive audio fatigue.
* **Delayed Execution:** Integrated `PlaySFXDelayed` for perfect synchronization with DOTween animations.

---

## 🛠️ Technology Stack

| Technology | Usage |
| :--- | :--- |
| **Built-in Render Pipeline** | High-compatibility, low-overhead rendering engine. |
| **DOTween (Demigiant)** | Procedural animations, UI transitions, and movement sequences. |
| **ShaderLab / HLSL** | Custom SDF grid and environment shaders. |
| **ScriptableObjects** | Level Data, Color Catalogs, and Sound Databases. |
| **Singleton / Context** | Centralized manager initialization and service registration. |

---

## 📖 Level Editor Guide (Case Feature)

<details>
<summary><b>Click to expand: How to create new levels in seconds</b></summary>

### 1️⃣ Initialization
1. Open `Assets/Scenes/Editor/LevelEditor.unity`.
2. Select the `LevelEditorContext` in the Hierarchy.
3. Click **"Create New Level SO"** to generate a fresh data file.

### 2️⃣ Painting the Grid
Select a brush from the Inspector grid:
* **Passenger Brushes:** Click to place color codes.
* **Obstacle Brush:** Block paths for strategic design.
* **Eraser:** Quickly remove objects or entire tiles.

### 3️⃣ Logistics & Validation
* **Bus Sequence:** Define the arrival order of buses in the Inspector list.
* **Auto-Validator:** The editor provides real-time feedback if:
    * Passenger counts aren't multiples of 3.
    * The bus sequence doesn't match the required passenger capacity.

### 4️⃣ Publishing
Click **"PUBLISH / SAVE LEVEL"**. Your level is saved to `Resources/Levels` and is ready for the `GameManager` to load.
</details>

---

## 🎮 Getting Started

### Play the Game
1.  **Menu Flow:** Start from `MenuScene` for full persistent manager initialization.
2.  **Dev Flow:** Start from `GameScene` and assign a `testLevelOverride` in the `GameManager` Inspector for rapid testing.