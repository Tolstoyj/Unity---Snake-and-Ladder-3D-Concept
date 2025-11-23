using UnityEngine;
using System.Collections;

public class AIPlayer : MonoBehaviour
{
    private GameManager gameManager;
    private Player player;
    private float thinkDelay = 1f; // AI thinking time
    
    public void Initialize(GameManager manager, Player playerData)
    {
        gameManager = manager;
        player = playerData;
    }
    
    public void TakeTurn()
    {
        StartCoroutine(AITurnCoroutine());
    }
    
    private IEnumerator AITurnCoroutine()
    {
        // AI thinking delay
        yield return new WaitForSeconds(thinkDelay);
        
        // Roll the dice
        if (gameManager != null)
        {
            gameManager.RollDiceForCurrentPlayer();
        }
    }
    
    public void SetThinkDelay(float delay)
    {
        thinkDelay = delay;
    }
}

