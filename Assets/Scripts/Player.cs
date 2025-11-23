using UnityEngine;

[System.Serializable]
public class Player
{
    public int playerID;
    public string playerName;
    public bool isAI;
    public GameObject playerObject;
    public int currentSquare;
    public bool hasStarted; // Has the player rolled a 1 to start?
    public bool isActive; // Is the player still in the game?
    
    public Player(int id, string name, bool ai, GameObject playerObj)
    {
        playerID = id;
        playerName = name;
        isAI = ai;
        playerObject = playerObj;
        currentSquare = 0; // 0 means not on board yet
        hasStarted = false;
        isActive = true;
    }
    
    public void Reset()
    {
        currentSquare = 0;
        hasStarted = false;
        isActive = true;
    }
    
    public void StartGame()
    {
        hasStarted = true;
        currentSquare = 1;
    }
    
    public void MoveToSquare(int square)
    {
        currentSquare = square;
    }
}

