# Snake and Ladder Game - Unity Project

A complete implementation of the traditional Indian Snake and Ladder board game in Unity, featuring a 100-square board, dice rolling mechanics, player movement, and AI support.

![Game Board](Screenshots/Screenshot%202025-11-23%20at%2001.40.25.png)
*The complete 100-square game board with numbered squares*

![Game Manager Setup](Screenshots/Screenshot%202025-11-23%20at%2001.40.33.png)
*GameManager component configuration in the Unity Inspector*

![Gameplay View](Screenshots/Screenshot%202025-11-23%20at%2001.40.56.png)
*In-game view showing the board, dice, and player capsules*

## 📋 Table of Contents

- [Features](#features)
- [Screenshots](#screenshots)
- [Requirements](#requirements)
- [Setup Instructions](#setup-instructions)
- [Game Components](#game-components)
- [How to Play](#how-to-play)
- [Game Rules](#game-rules)
- [Troubleshooting](#troubleshooting)

## ✨ Features

- **100-Square Board**: Traditional zigzag pattern board with numbered squares
- **Dice Rolling**: 3D dice with dots (pips) on each face
- **Player Management**: Support for 2-10 players
- **AI Players**: Optional AI opponents
- **Turn-Based System**: Automatic turn management
- **Smooth Animations**: Player movement and dice rolling animations
- **Multiple Input Methods**: Spacebar or mouse click to roll dice
- **Snakes and Ladders**: Traditional snake and ladder mechanics with automatic movement
- **Visual Indicators**: Green lines for ladders, red lines for snakes
- **Traditional Configuration**: Pre-configured traditional snake/ladder positions

## 📸 Screenshots

### Game Board
![Game Board](Screenshots/Screenshot%202025-11-23%20at%2001.40.25.png)
The complete 100-square game board with numbered squares arranged in the traditional zigzag pattern.

### Game Manager Configuration
![Game Manager Setup](Screenshots/Screenshot%202025-11-23%20at%2001.40.33.png)
GameManager component showing player settings, game rules, and component references.

### Gameplay View
![Gameplay View](Screenshots/Screenshot%202025-11-23%20at%2001.40.56.png)
In-game view showing the board, dice, and player capsules during gameplay.

## 📦 Requirements

- Unity 6.2 or later
- Input System Package (included in Unity 6.2+)
- Universal Render Pipeline (URP) - already configured

## 🚀 Setup Instructions

### Step 1: Open the Project

1. Open Unity Hub
2. Click "Add" and select the project folder
3. Open the project in Unity

### Step 2: Generate the Board

1. In the Hierarchy, find or create a GameObject named "Board"
2. Add the `SnakeLadderBoardGenerator` component to it
3. In the Inspector, right-click the `SnakeLadderBoardGenerator` component
4. Select **"Generate Board"**
5. Wait for the board to generate (you'll see 100 squares appear)

**Note**: If you see "missing script" errors:
- Right-click the `SnakeLadderBoardGenerator` component
- Select **"Fix Missing Scripts"**

### Step 3: Generate the Dice

1. In the Hierarchy, find or create a GameObject named "Dice"
2. Add the `DiceGenerator` component to it
3. In the Inspector, configure the dice settings (optional):
   - **Dice Size**: Size of the dice cube
   - **Dice Color**: Color of the dice
   - **Dot Settings**: Customize dot appearance
4. Right-click the `DiceGenerator` component
5. Select **"Generate Dice"**

**Note**: If dice clicking doesn't work:
- Right-click the `DiceGenerator` component
- Select **"Fix Click Handler"**

### Step 4: Setup Snake and Ladder Manager

1. In the Hierarchy, create an empty GameObject named "SnakeLadderManager"
2. Add the `SnakeLadderManager` component to it
3. Configure snakes and ladders:
   - **Option A (Recommended)**: Right-click the `SnakeLadderManager` component → Select **"Create Traditional Snakes and Ladders"**
   - **Option B (Custom)**: In the Inspector, expand "Snakes And Ladders Configuration" and manually add entries
4. Right-click the component again → Select **"Initialize Snakes and Ladders"**
5. Visual lines will appear connecting the squares (green for ladders, red for snakes)

### Step 5: Setup Game Manager

1. In the Hierarchy, create an empty GameObject named "GameManager"
2. Add the `GameManager` component to it
3. In the Inspector, configure the game settings:

   **Game Settings:**
   - **Number Of Players**: Set between 2-10
   - **Number Of AI Players**: Set how many players should be AI (0 to Number Of Players)

   **References:**
   - **Board Generator**: Drag the "Board" GameObject here
   - **Dice Generator**: Drag the "Dice" GameObject here
   - **Snake Ladder Manager**: Drag the "SnakeLadderManager" GameObject here
   - **Player Prefab**: (Optional) Drag a player prefab, or leave empty to use default capsule

   **Player Settings:**
   - **Player Names**: Customize player names (default: Player 1, Player 2, etc.)
   - **Player Colors**: Set colors for each player

   **Game Rules:**
   - **Starting Dice Value**: Must roll this number to start (default: 1)
   - **Winning Square**: Square number to win (default: 100)
   - **Allow Extra Turn On Six**: If true, rolling 6 gives an extra turn

### Step 5: Setup Camera

1. Ensure you have a **Main Camera** in the scene
2. Position it to view the board
3. The camera should be tagged as "MainCamera" (default)

### Step 6: Start the Game

1. Enter **Play Mode** (click the Play button)
2. Select the "GameManager" GameObject
3. Right-click the `GameManager` component
4. Select **"Start Game"**

## 🎮 Game Components

### SnakeLadderBoardGenerator

**Purpose**: Creates the 100-square game board

**Key Methods:**
- `GenerateBoard()`: Creates the board with 100 squares
- `ClearBoard()`: Removes all squares
- `FixMissingScripts()`: Fixes missing SquareData components
- `GetSquarePosition(int squareNumber)`: Gets the world position of a square (1-100)
- `IsBoardGenerated()`: Checks if the board exists

**Settings:**
- **Board Size**: 10x10 grid (100 squares)
- **Square Size**: Size of each square
- **Spacing**: Space between squares
- **Colors**: Even/odd square colors for checkerboard pattern

### SnakeLadderManager

**Purpose**: Manages snakes and ladders on the board

**Key Methods:**
- `CreateTraditionalSnakesAndLadders()`: Creates traditional snake/ladder positions
- `InitializeSnakesAndLadders()`: Initializes and validates all snakes/ladders
- `HasSnakeOrLadder(int squareNumber)`: Checks if a square has a snake/ladder
- `GetDestination(int squareNumber)`: Gets the destination square
- `AddSnakeLadder(int start, int end, bool isLadder)`: Adds a custom snake/ladder

**Settings:**
- **Snakes And Ladders Configuration**: List of all snakes and ladders
- **Show Visual Indicators**: Enable/disable visual lines
- **Ladder Color**: Color for ladder lines (default: green)
- **Snake Color**: Color for snake lines (default: red)
- **Line Width**: Thickness of indicator lines
- **Line Height**: Height above board for lines

**Context Menu Options:**
- **Create Traditional Snakes and Ladders**: Sets up traditional positions
- **Initialize Snakes and Ladders**: Validates and creates visual indicators
- **Clear All Snakes and Ladders**: Removes all snakes/ladders

### DiceGenerator

**Purpose**: Creates and manages the 3D dice

**Key Methods:**
- `GenerateDice()`: Creates the dice with dots
- `ClearDice()`: Removes the dice
- `FixClickHandler()`: Fixes the click detection
- `RollDiceAnimated(float duration)`: Rolls the dice with animation
- `GetDiceValue()`: Gets the current face value (1-6)

**Settings:**
- **Dice Size**: Size of the dice cube
- **Dot Settings**: Size, color, and spacing of dots
- **Click Settings**: Enable/disable click to roll

### GameManager

**Purpose**: Manages game flow, turns, and player actions

**Key Methods:**
- `StartGame()`: Begins the game
- `ResetGame()`: Resets all players to start
- `RollDiceForCurrentPlayer()`: Rolls dice for current player
- `GetCurrentPlayer()`: Gets the current player's data

**Settings:**
- **Number Of Players**: 2-10 players
- **Number Of AI Players**: How many are AI
- **Game Rules**: Starting dice value, winning square, extra turn on 6

### PlayerMovement

**Purpose**: Handles player movement on the board

**Key Methods:**
- `MovePlayer(int steps)`: Moves player by number of steps
- `SetPosition(int squareNumber)`: Teleports player to a square
- `GetCurrentSquare()`: Gets current square number

**Settings:**
- **Move Speed**: Speed of movement between squares
- **Rotation Speed**: Speed of rotation
- **Height Offset**: Height above the board

### Player (Data Class)

**Purpose**: Stores player information

**Properties:**
- `playerID`: Unique ID
- `playerName`: Player's name
- `isAI`: Whether player is AI
- `currentSquare`: Current position (0 = off board, 1-100 = on board)
- `hasStarted`: Whether player has rolled the starting number

### AIPlayer

**Purpose**: Controls AI player behavior

**Settings:**
- **Think Delay**: Time before AI rolls (default: 1 second)

## 🎲 How to Play

### Starting the Game

1. Make sure the board and dice are generated
2. Configure GameManager settings
3. Enter Play Mode
4. Right-click GameManager → "Start Game"

### During Your Turn

**To Roll the Dice:**
- Press **SPACEBAR**, OR
- **Click the dice** (left or right mouse button)

**What Happens:**
1. If you haven't started: You must roll **1** to enter the board
2. If you've started: You move that many squares forward
3. Rolling **6** gives you an extra turn (if enabled)
4. First to reach square **100** wins!

### AI Players

- AI players roll automatically after a short delay
- No input needed for AI turns

## 📜 Game Rules

### Starting Rule
- Players must roll **1** (or the configured starting value) to enter the board
- Until you roll 1, your turn is skipped

### Movement
- Once started, roll the dice and move forward that many squares
- Follow the board's zigzag pattern automatically
- Cannot move beyond square 100

### Extra Turn
- If enabled, rolling **6** gives you another turn immediately

### Winning
- First player to reach or exceed square **100** wins
- The game ends and a winner is announced

### Turn Order
- Players take turns in order (Player 1, Player 2, Player 3, etc.)
- Turn automatically advances after each roll (unless extra turn)

## 🔧 Troubleshooting

### "Board has not been generated!"
**Solution:**
1. Select the Board GameObject
2. Right-click `SnakeLadderBoardGenerator` component
3. Select "Generate Board"

### "The referenced script (Unknown) is missing!"
**Solution:**
1. Select the Board GameObject
2. Right-click `SnakeLadderBoardGenerator` component
3. Select "Fix Missing Scripts"

### "DiceGenerator not found!"
**Solution:**
1. Make sure the Dice GameObject exists
2. Assign it in GameManager's "Dice Generator" field
3. Or ensure it's in the scene and will be auto-found

### "Dice object is null!"
**Solution:**
1. Select the Dice GameObject
2. Right-click `DiceGenerator` component
3. Select "Generate Dice"

### Spacebar not working
**Solution:**
1. Make sure you're in **Play Mode**
2. Check Project Settings → Player → Active Input Handling
3. Set to "Input System Package (New)" or "Both"
4. Make sure the game is started and it's your turn

### Mouse click on dice not working
**Solution:**
1. Select the Dice GameObject
2. Right-click `DiceGenerator` component
3. Select "Fix Click Handler"
4. Make sure the dice has a collider (should be automatic)
5. Make sure you have a Camera in the scene

### "Not enough players!"
**Solution:**
1. In GameManager, set "Number Of Players" to at least 2
2. Maximum is 10 players

### Players not moving
**Solution:**
1. Make sure the board is generated
2. Check that PlayerMovement component is on player objects
3. Verify board generator is assigned in GameManager
4. Check console for error messages

### Snakes and ladders not working
**Solution:**
1. Make sure SnakeLadderManager GameObject exists
2. Right-click SnakeLadderManager component → "Create Traditional Snakes and Ladders"
3. Right-click again → "Initialize Snakes and Ladders"
4. Assign SnakeLadderManager to GameManager's "Snake Ladder Manager" field
5. Test using GameManager context menu → "Test Snake/Ladder System"
6. Check console for "SnakeLadderManager is null!" warnings

### Input System Warnings
**Solution:**
1. Go to Edit → Project Settings → Player
2. Find "Active Input Handling"
3. Set to "Input System Package (New)" or "Both"
4. Restart Unity if needed

## 📝 Scripts Overview

### Core Scripts
- `SnakeLadderBoardGenerator.cs`: Board creation and management
- `DiceGenerator.cs`: Dice creation and rolling
- `GameManager.cs`: Main game controller
- `PlayerMovement.cs`: Player movement system
- `Player.cs`: Player data structure
- `AIPlayer.cs`: AI player controller

### Helper Classes
- `SquareData.cs`: Stores square information
- `DiceData.cs`: Stores dice information
- `DiceClickHandler.cs`: Handles dice click detection

## 🎯 Quick Start Checklist

- [ ] Board generated (right-click SnakeLadderBoardGenerator → Generate Board)
- [ ] Dice generated (right-click DiceGenerator → Generate Dice)
- [ ] SnakeLadderManager created and initialized (right-click → Create Traditional Snakes and Ladders → Initialize)
- [ ] GameManager configured (players, references set including SnakeLadderManager)
- [ ] Camera in scene (tagged as MainCamera)
- [ ] Enter Play Mode
- [ ] Start Game (right-click GameManager → Start Game)
- [ ] Press SPACEBAR or click dice to roll
- [ ] Watch players automatically move on snakes and ladders!

## 🎨 Customization

### Board Appearance
- Adjust square colors in `SnakeLadderBoardGenerator`
- Change square size and spacing
- Modify number text size and color

### Dice Appearance
- Change dice size and color
- Adjust dot size, color, and spacing
- Customize roll animation duration

### Player Appearance
- Set player colors in GameManager
- Use custom player prefabs
- Adjust movement speed and rotation

### Game Rules
- Change starting dice value
- Modify winning square
- Enable/disable extra turn on 6
- Adjust AI thinking delay

## 📚 Additional Notes

- The board uses a traditional zigzag pattern (left-right, right-left alternating rows)
- Square 1 is at the bottom-left, square 100 is at the top-left
- Players start off the board (position 0) until they roll the starting number
- All scripts use object-oriented design principles
- The game supports both human and AI players simultaneously
- Snakes and ladders are processed automatically after movement completes
- Visual indicators (lines) show snake/ladder connections above the board
- Snakes always go down, ladders always go up (validated automatically)
- Multiple snakes/ladders can chain if destination also has one (with safety limit)

## 🤝 Contributing

Feel free to extend this project with:
- Snake and ladder mechanics (teleport squares)
- UI system for better player experience
- Sound effects and music
- Multiplayer networking
- Save/load game state
- Different board themes

## 📄 License

This project is provided as-is for educational purposes.

## 🔗 Repository

This project is available on GitHub: [Unity - Snake and Ladder 3D Concept](https://github.com/Tolstoyj/Unity---Snake-and-Ladder-3D-Concept.git)

---

**Enjoy playing Snake and Ladder! 🎲🎮**

