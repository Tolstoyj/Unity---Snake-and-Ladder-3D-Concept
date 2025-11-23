using UnityEngine;
using System;

[System.Serializable]
public class SnakeLadder
{
    public int startSquare;
    public int endSquare;
    public bool isLadder; // true = ladder (goes up), false = snake (goes down)
    public string name; // Optional name for the snake/ladder
    
    public SnakeLadder(int start, int end, bool ladder)
    {
        startSquare = start;
        endSquare = end;
        isLadder = ladder;
        name = ladder ? "Ladder" : "Snake";
    }
    
    // Validate that ladder goes up and snake goes down
    public bool IsValid()
    {
        if (isLadder)
        {
            return endSquare > startSquare; // Ladder must go up
        }
        else
        {
            return endSquare < startSquare; // Snake must go down
        }
    }
}

