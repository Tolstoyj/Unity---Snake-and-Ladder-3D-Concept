using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int numberOfPlayers = 2;
    [SerializeField] private int numberOfAIPlayers = 0;
    
    [Header("References")]
    [SerializeField] private SnakeLadderBoardGenerator boardGenerator;
    [SerializeField] private DiceGenerator diceGenerator;
    [SerializeField] private SnakeLadderManager snakeLadderManager;
    [SerializeField] private GameObject playerPrefab; // Prefab for player capsule
    
    [Header("Player Settings")]
    [SerializeField] private string[] playerNames = { "Player 1", "Player 2", "Player 3", "Player 4", "Player 5", "Player 6", "Player 7", "Player 8", "Player 9", "Player 10" };
    [SerializeField] private Color[] playerColors = new Color[10];
    
    [Header("Game Rules")]
    [SerializeField] private int startingDiceValue = 1; // Must roll 1 to start
    [SerializeField] private int winningSquare = 100;
    [SerializeField] private bool allowExtraTurnOnSix = true; // Roll 6 to get extra turn
    
    [Header("UI Debug")]
    [SerializeField] private bool showDebugInfo = true;
    
    private List<Player> players = new List<Player>();
    private int currentPlayerIndex = 0;
    private bool isGameActive = false;
    private bool isWaitingForDiceRoll = false;
    private int lastDiceValue = 0;
    private Dictionary<int, AIPlayer> aiPlayers = new Dictionary<int, AIPlayer>();
    
    // Events
    public System.Action<Player> OnPlayerTurnStarted;
    public System.Action<Player, int> OnPlayerMoved;
    public System.Action<Player> OnPlayerWon;
    public System.Action<int> OnDiceRolled;
    
    private void Start()
    {
        InitializeGame();
        ConnectDiceGenerator();
    }
    
    private void Update()
    {
        // Check for spacebar to roll dice
        if (isGameActive && isWaitingForDiceRoll)
        {
            bool spacePressed = false;
            
            // Try new Input System first
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                spacePressed = keyboard.spaceKey.wasPressedThisFrame;
            }
            else if (showDebugInfo && Time.frameCount % 300 == 0)
            {
                Debug.LogWarning("Keyboard.current is null! Make sure Input System is enabled in Project Settings.");
            }
            
            if (spacePressed)
            {
                // Only allow if current player is not AI
                Player currentPlayer = GetCurrentPlayer();
                if (currentPlayer != null && !currentPlayer.isAI)
                {
                    if (showDebugInfo)
                    {
                        Debug.Log("Spacebar pressed! Rolling dice...");
                    }
                    RollDiceForCurrentPlayer();
                }
                else if (showDebugInfo)
                {
                    Debug.LogWarning($"Cannot roll: Current player is {(currentPlayer == null ? "null" : (currentPlayer.isAI ? "AI" : "unknown"))}.");
                }
            }
        }
    }
    
    private void ConnectDiceGenerator()
    {
        if (diceGenerator == null)
        {
            diceGenerator = FindFirstObjectByType<DiceGenerator>();
        }
        
        if (diceGenerator != null)
        {
            // Connect dice generator to this game manager
            diceGenerator.gameManager = this;
            if (showDebugInfo)
            {
                Debug.Log("DiceGenerator connected to GameManager!");
            }
        }
        else
        {
            Debug.LogWarning("DiceGenerator not found! Dice rolling may not work.");
        }
    }
    
    private void InitializeGame()
    {
        // Find references if not assigned
        if (boardGenerator == null)
        {
            boardGenerator = FindFirstObjectByType<SnakeLadderBoardGenerator>();
        }
        
        if (diceGenerator == null)
        {
            diceGenerator = FindFirstObjectByType<DiceGenerator>();
        }
        
        if (snakeLadderManager == null)
        {
            snakeLadderManager = FindFirstObjectByType<SnakeLadderManager>();
            if (snakeLadderManager != null && showDebugInfo)
            {
                Debug.Log("SnakeLadderManager found and connected!");
            }
        }
        
        // Validate board
        if (boardGenerator == null)
        {
            Debug.LogError("SnakeLadderBoardGenerator not found! Please assign it in the Inspector.");
            return;
        }
        
        if (!boardGenerator.IsBoardGenerated())
        {
            Debug.LogWarning("Board has not been generated! Please generate the board first. Players will be created when board is ready.");
            return;
        }
        
        // Validate player count
        if (numberOfPlayers < 2 || numberOfPlayers > 10)
        {
            Debug.LogWarning($"Number of players ({numberOfPlayers}) is invalid! Clamping to valid range (2-10).");
            numberOfPlayers = Mathf.Clamp(numberOfPlayers, 2, 10);
        }
        
        if (numberOfAIPlayers > numberOfPlayers)
        {
            numberOfAIPlayers = numberOfPlayers;
        }
        
        // Initialize default colors if not set
        if (playerColors == null || playerColors.Length == 0 || playerColors[0] == Color.clear)
        {
            InitializeDefaultColors();
        }
        
        // Only create players if board is ready
        if (boardGenerator.IsBoardGenerated())
        {
            CreatePlayers();
            
            if (showDebugInfo)
            {
                Debug.Log($"Game initialized with {numberOfPlayers} players ({numberOfAIPlayers} AI)");
            }
        }
    }
    
    private void InitializeDefaultColors()
    {
        playerColors[0] = Color.red;
        playerColors[1] = Color.blue;
        playerColors[2] = Color.green;
        playerColors[3] = Color.yellow;
        playerColors[4] = Color.magenta;
        playerColors[5] = Color.cyan;
        playerColors[6] = new Color(1f, 0.5f, 0f); // Orange
        playerColors[7] = new Color(0.5f, 0f, 1f); // Purple
        playerColors[8] = new Color(1f, 0.75f, 0.8f); // Pink
        playerColors[9] = new Color(0.5f, 0.5f, 0.5f); // Gray
    }
    
    private void CreatePlayers()
    {
        // Clear existing players and destroy their GameObjects
        foreach (Player player in players)
        {
            if (player.playerObject != null)
            {
                DestroyImmediate(player.playerObject);
            }
        }
        players.Clear();
        aiPlayers.Clear();
        
        for (int i = 0; i < numberOfPlayers; i++)
        {
            bool isAI = i < numberOfAIPlayers;
            string name = playerNames[i];
            
            // Create player GameObject
            GameObject playerObj = null;
            if (playerPrefab != null)
            {
                playerObj = Instantiate(playerPrefab);
            }
            else
            {
                // Create default capsule
                playerObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                playerObj.name = name;
                
                // Set color
                Renderer renderer = playerObj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = playerColors[i];
                }
            }
            
            // Position at starting point (off board)
            Vector3 startPos = boardGenerator.GetSquarePosition(1);
            if (startPos != Vector3.zero)
            {
                playerObj.transform.position = new Vector3(startPos.x - 2f, startPos.y + 0.5f, startPos.z);
            }
            
            // Add PlayerMovement component
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement == null)
            {
                playerMovement = playerObj.AddComponent<PlayerMovement>();
            }
            
            // Disable keyboard input for GameManager-controlled players
            playerMovement.allowKeyboardInput = false;
            
            // Set position off board (0 = off board)
            playerMovement.SetPosition(0);
            
            // Create Player data
            Player player = new Player(i + 1, name, isAI, playerObj);
            players.Add(player);
            
            // Create AI component if needed
            if (isAI)
            {
                AIPlayer aiPlayer = playerObj.AddComponent<AIPlayer>();
                aiPlayer.Initialize(this, player);
                aiPlayers[i] = aiPlayer;
            }
        }
    }
    
    [ContextMenu("Start Game")]
    public void StartGame()
    {
        // Ensure players are created
        if (players.Count == 0)
        {
            Debug.LogWarning("Players not created yet. Creating players now...");
            CreatePlayers();
        }
        
        if (players.Count < 2)
        {
            Debug.LogError($"Not enough players! Have {players.Count}, need at least 2 players. Check Number Of Players setting.");
            return;
        }
        
        if (!boardGenerator.IsBoardGenerated())
        {
            Debug.LogError("Board not generated! Cannot start game. Please generate the board first.");
            return;
        }
        
        if (diceGenerator == null)
        {
            Debug.LogWarning("DiceGenerator not found! Dice rolling may not work.");
        }
        
        isGameActive = true;
        currentPlayerIndex = 0;
        
        if (showDebugInfo)
        {
            Debug.Log($"Game Started! {players.Count} players ready.");
        }
        
        StartNextTurn();
    }
    
    [ContextMenu("Reset Game")]
    public void ResetGame()
    {
        isGameActive = false;
        isWaitingForDiceRoll = false;
        currentPlayerIndex = 0;
        lastDiceValue = 0;
        
        foreach (Player player in players)
        {
            player.Reset();
            if (player.playerObject != null)
            {
                PlayerMovement movement = player.playerObject.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.SetPosition(0);
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Game reset!");
        }
    }
    
    private void StartNextTurn()
    {
        if (!isGameActive)
            return;
        
        // Find next active player
        int attempts = 0;
        while (attempts < players.Count)
        {
            Player currentPlayer = players[currentPlayerIndex];
            
            if (currentPlayer.isActive)
            {
                if (showDebugInfo)
                {
                    Debug.Log($"=== {currentPlayer.playerName}'s Turn ===");
                }
                
                OnPlayerTurnStarted?.Invoke(currentPlayer);
                
                // Set waiting for dice roll for both AI and human players
                isWaitingForDiceRoll = true;
                
                // If AI player, let AI take turn
                if (currentPlayer.isAI)
                {
                    if (aiPlayers.ContainsKey(currentPlayerIndex))
                    {
                        aiPlayers[currentPlayerIndex].TakeTurn();
                    }
                    else
                    {
                        Debug.LogWarning($"AI player {currentPlayer.playerName} has no AIPlayer component!");
                        isWaitingForDiceRoll = false;
                    }
                }
                else
                {
                    // Human player - wait for dice roll
                    PromptPlayerToRoll();
                }
                
                return;
            }
            
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            attempts++;
        }
        
        // No active players found
        Debug.LogWarning("No active players found!");
        isGameActive = false;
    }
    
    private void PromptPlayerToRoll()
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        if (showDebugInfo)
        {
            if (!currentPlayer.hasStarted)
            {
                Debug.Log($"{currentPlayer.playerName}: Press SPACEBAR or click the dice to roll! You need to get {startingDiceValue} to start.");
            }
            else
            {
                Debug.Log($"{currentPlayer.playerName}: Press SPACEBAR or click the dice to roll and move!");
            }
        }
    }
    
    // Called when dice is rolled (from DiceGenerator or manual call)
    public void HandleDiceRoll(int diceValue)
    {
        if (!isGameActive || !isWaitingForDiceRoll)
            return;
        
        lastDiceValue = diceValue;
        OnDiceRolled?.Invoke(diceValue);
        
        Player currentPlayer = players[currentPlayerIndex];
        
        if (showDebugInfo)
        {
            Debug.Log($"{currentPlayer.playerName} rolled: {diceValue}");
        }
        
        ProcessDiceRoll(diceValue);
    }
    
    // Public method to roll dice for current player (can be called from UI or dice click)
    public void RollDiceForCurrentPlayer()
    {
        if (diceGenerator == null)
        {
            Debug.LogError("DiceGenerator not found! Cannot roll dice.");
            return;
        }
        
        if (!isGameActive)
        {
            Debug.LogWarning("Game is not active! Cannot roll dice.");
            return;
        }
        
        if (!isWaitingForDiceRoll)
        {
            Debug.LogWarning("Not waiting for dice roll! Cannot roll dice.");
            return;
        }
        
        // Check if dice object exists
        if (diceGenerator.diceObject == null)
        {
            Debug.LogError("Dice object is null! Please generate the dice first.");
            return;
        }
        
        if (showDebugInfo)
        {
            Debug.Log("Rolling dice for current player...");
        }
        
        // Roll dice - the DiceGenerator will notify us via HandleDiceRoll when complete
        diceGenerator.RollDiceAnimated(1f);
    }
    
    private void ProcessDiceRoll(int diceValue)
    {
        Player currentPlayer = players[currentPlayerIndex];
        
        // Check if player needs to start
        if (!currentPlayer.hasStarted)
        {
            if (diceValue == startingDiceValue)
            {
                // Player can start!
                currentPlayer.StartGame();
                MovePlayerToSquare(currentPlayer, 1);
                
                if (showDebugInfo)
                {
                    Debug.Log($"{currentPlayer.playerName} started! They are now on square 1.");
                }
                
                // Check for snake or ladder on square 1 (unlikely but possible)
                CheckSnakeOrLadder(currentPlayer);
                
                // Player gets another turn after starting
                isWaitingForDiceRoll = false;
                StartNextTurn();
            }
            else
            {
                // Player didn't get the required number to start
                if (showDebugInfo)
                {
                    Debug.Log($"{currentPlayer.playerName} rolled {diceValue}. Need {startingDiceValue} to start. Turn passed.");
                }
                
                // Move to next player
                isWaitingForDiceRoll = false;
                currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
                StartNextTurn();
            }
        }
        else
        {
            // Player is already on the board, move them
            int newSquare = currentPlayer.currentSquare + diceValue;
            
            // Check for win condition
            if (newSquare >= winningSquare)
            {
                newSquare = winningSquare;
                MovePlayerToSquare(currentPlayer, newSquare);
                PlayerWon(currentPlayer);
                return;
            }
            
            MovePlayerToSquare(currentPlayer, newSquare);
            
            // Wait for movement to complete, then check for snake/ladder
            StartCoroutine(WaitForMovementAndCheckSnakeLadder(currentPlayer, diceValue));
            
            return; // Exit early, the coroutine will handle turn progression
        }
    }
    
    private IEnumerator WaitForMovementAndCheckSnakeLadder(Player player, int diceValue)
    {
        // Wait for player movement to complete
        PlayerMovement movement = player.playerObject?.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            while (movement.IsMoving())
            {
                yield return null;
            }
            
            // Small delay to ensure position is updated
            yield return new WaitForSeconds(0.1f);
        }
        
        // Now check for snake/ladder
        CheckSnakeOrLadder(player);
        
        // Wait again if snake/ladder moved the player
        if (movement != null)
        {
            yield return new WaitForSeconds(0.2f); // Give time for teleport animation
        }
        
        // Check for extra turn (if rolled 6)
        if (allowExtraTurnOnSix && diceValue == 6)
        {
            if (showDebugInfo)
            {
                Debug.Log($"{player.playerName} rolled 6! Extra turn!");
            }
            
            isWaitingForDiceRoll = false;
            StartNextTurn(); // Same player goes again
        }
        else
        {
            // Move to next player
            isWaitingForDiceRoll = false;
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
            StartNextTurn();
        }
    }
    
    private void CheckSnakeOrLadder(Player player)
    {
        if (snakeLadderManager == null)
        {
            if (showDebugInfo)
            {
                Debug.LogWarning("SnakeLadderManager is null! Snakes and ladders will not work.");
            }
            return;
        }
        
        int currentSquare = player.currentSquare;
        int maxChecks = 10; // Safety limit to prevent infinite loops
        int checkCount = 0;
        
        if (showDebugInfo)
        {
            Debug.Log($"Checking snake/ladder for {player.playerName} on square {currentSquare}");
        }
        
        // Keep checking for snakes/ladders until player lands on a normal square
        while (snakeLadderManager.HasSnakeOrLadder(currentSquare) && checkCount < maxChecks)
        {
            SnakeLadder sl = snakeLadderManager.GetSnakeLadder(currentSquare);
            int destination = snakeLadderManager.GetDestination(currentSquare);
            
            if (sl != null && destination != currentSquare)
            {
                if (showDebugInfo)
                {
                    string message = sl.isLadder 
                        ? $"{player.playerName} found a LADDER! Climbing from square {currentSquare} to square {destination}!"
                        : $"{player.playerName} hit a SNAKE! Sliding down from square {currentSquare} to square {destination}!";
                    Debug.Log(message);
                }
                
                // Move player to destination (teleport, not step-by-step)
                TeleportPlayerToSquare(player, destination);
                currentSquare = destination;
                player.MoveToSquare(destination); // Update player data
                checkCount++;
            }
            else
            {
                break;
            }
        }
        
        if (checkCount >= maxChecks)
        {
            Debug.LogWarning($"Snake/Ladder check limit reached for {player.playerName}! Possible infinite loop detected.");
        }
        else if (checkCount == 0 && showDebugInfo)
        {
            Debug.Log($"No snake/ladder found on square {currentSquare} for {player.playerName}");
        }
    }
    
    // Teleport player directly to a square (for snakes/ladders)
    private void TeleportPlayerToSquare(Player player, int square)
    {
        if (player.playerObject == null)
            return;
        
        PlayerMovement movement = player.playerObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            // Use SetPosition to teleport directly
            movement.SetPosition(square);
        }
        
        player.MoveToSquare(square);
        OnPlayerMoved?.Invoke(player, square);
    }
    
    private void MovePlayerToSquare(Player player, int square)
    {
        if (player.playerObject == null)
            return;
        
        PlayerMovement movement = player.playerObject.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            int steps = square - player.currentSquare;
            if (steps > 0)
            {
                movement.MovePlayer(steps);
            }
            else
            {
                movement.SetPosition(square);
            }
        }
        
        player.MoveToSquare(square);
        OnPlayerMoved?.Invoke(player, square);
    }
    
    private void PlayerWon(Player player)
    {
        isGameActive = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"🎉 {player.playerName} WINS! 🎉");
        }
        
        OnPlayerWon?.Invoke(player);
    }
    
    // Getters
    public Player GetCurrentPlayer()
    {
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count)
        {
            return players[currentPlayerIndex];
        }
        return null;
    }
    
    public List<Player> GetAllPlayers()
    {
        return new List<Player>(players);
    }
    
    public bool IsGameActive()
    {
        return isGameActive;
    }
    
    public bool IsWaitingForDiceRoll()
    {
        return isWaitingForDiceRoll;
    }
    
    // Settings
    public void SetNumberOfPlayers(int count)
    {
        numberOfPlayers = Mathf.Clamp(count, 2, 10);
    }
    
    public void SetNumberOfAIPlayers(int count)
    {
        numberOfAIPlayers = Mathf.Clamp(count, 0, numberOfPlayers);
    }
    
    [ContextMenu("Test Input System")]
    public void TestInputSystem()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            Debug.Log($"Input System is working! Keyboard found. Space key state: {keyboard.spaceKey.isPressed}");
        }
        else
        {
            Debug.LogError("Input System NOT working! Keyboard.current is null. Check Project Settings > Player > Active Input Handling.");
        }
    }
    
    [ContextMenu("Test Snake/Ladder System")]
    public void TestSnakeLadderSystem()
    {
        if (snakeLadderManager == null)
        {
            Debug.LogError("SnakeLadderManager is null! Please assign it in the Inspector.");
            return;
        }
        
        var snakesLadders = snakeLadderManager.GetAllSnakesAndLadders();
        Debug.Log($"Total snakes and ladders configured: {snakesLadders.Count}");
        
        // Test a few squares
        int[] testSquares = { 2, 8, 29, 38, 47 };
        foreach (int square in testSquares)
        {
            bool hasSnakeLadder = snakeLadderManager.HasSnakeOrLadder(square);
            if (hasSnakeLadder)
            {
                int destination = snakeLadderManager.GetDestination(square);
                SnakeLadder sl = snakeLadderManager.GetSnakeLadder(square);
                Debug.Log($"Square {square}: {(sl.isLadder ? "LADDER" : "SNAKE")} to square {destination}");
            }
            else
            {
                Debug.Log($"Square {square}: No snake/ladder");
            }
        }
    }
}

