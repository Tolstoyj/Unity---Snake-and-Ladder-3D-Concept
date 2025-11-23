using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Board Reference")]
    [SerializeField] private SnakeLadderBoardGenerator boardGenerator;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f; // Speed of movement between squares
    [SerializeField] private float rotationSpeed = 5f; // Speed of rotation
    [SerializeField] private float heightOffset = 0.5f; // Height above the board
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    public bool allowKeyboardInput = true; // Allow manual keyboard input (for testing)
    
    private int currentSquare = 1; // Start at square 1
    private bool isMoving = false;
    private Coroutine movementCoroutine;
    
    private void Start()
    {
        // Find board generator if not assigned
        if (boardGenerator == null)
        {
            boardGenerator = FindFirstObjectByType<SnakeLadderBoardGenerator>();
        }
        
        // Check if board exists and is generated
        if (boardGenerator == null)
        {
            Debug.LogError("SnakeLadderBoardGenerator not found! Please assign it in the Inspector or ensure a board generator exists in the scene.");
            return;
        }
        
        if (!boardGenerator.IsBoardGenerated())
        {
            Debug.LogError("Board has not been generated! Please generate the board first by right-clicking the board generator component and selecting 'Generate Board'.");
            return;
        }
        
        // Position player at starting square
        Vector3 startPos = boardGenerator.GetSquarePosition(1);
        if (startPos != Vector3.zero)
        {
            transform.position = new Vector3(startPos.x, startPos.y + heightOffset, startPos.z);
            if (showDebugInfo)
            {
                Debug.Log($"Player starting at square {currentSquare}");
            }
        }
        else
        {
            Debug.LogError("Could not get starting position! Make sure the board is generated.");
        }
    }
    
    private void Update()
    {
        // Check for keyboard input (1-6 keys) using new Input System
        // Only allow if keyboard input is enabled (for testing when not using GameManager)
        if (allowKeyboardInput && !isMoving)
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                MovePlayer(1);
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                MovePlayer(2);
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                MovePlayer(3);
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                MovePlayer(4);
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                MovePlayer(5);
            }
            else if (keyboard.digit6Key.wasPressedThisFrame)
            {
                MovePlayer(6);
            }
        }
    }
    
    public void MovePlayer(int steps)
    {
        if (isMoving)
            return;
        
        if (boardGenerator == null)
        {
            Debug.LogError("Board generator is not assigned!");
            return;
        }
        
        if (!boardGenerator.IsBoardGenerated())
        {
            Debug.LogError("Board has not been generated! Please generate the board first.");
            return;
        }
        
        int targetSquare = currentSquare + steps;
        
        // Clamp to board limits (1-100)
        if (targetSquare > 100)
        {
            targetSquare = 100;
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"Moving from square {currentSquare} to square {targetSquare} ({steps} steps)");
        }
        
        // Start movement coroutine
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        movementCoroutine = StartCoroutine(MoveToSquare(targetSquare));
    }
    
    private IEnumerator MoveToSquare(int targetSquare)
    {
        isMoving = true;
        
        while (currentSquare < targetSquare)
        {
            int nextSquare = currentSquare + 1;
            
            // Get positions
            Vector3 currentPos = boardGenerator.GetSquarePosition(currentSquare);
            Vector3 nextPos = boardGenerator.GetSquarePosition(nextSquare);
            
            if (currentPos == Vector3.zero || nextPos == Vector3.zero)
            {
                Debug.LogWarning($"Invalid square position: {currentSquare} or {nextSquare}");
                break;
            }
            
            // Add height offset
            currentPos.y += heightOffset;
            nextPos.y += heightOffset;
            
            // Calculate direction for rotation
            Vector3 direction = (nextPos - currentPos).normalized;
            direction.y = 0; // Keep rotation horizontal
            
            // Move and rotate step by step
            float distance = Vector3.Distance(currentPos, nextPos);
            float moveTime = distance / moveSpeed;
            float elapsed = 0f;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            while (elapsed < moveTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveTime;
                
                // Smooth movement
                transform.position = Vector3.Lerp(currentPos, nextPos, t);
                
                // Smooth rotation
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                
                yield return null;
            }
            
            // Ensure we're exactly at the target position
            transform.position = nextPos;
            transform.rotation = targetRotation;
            
            currentSquare = nextSquare;
            
            // Small delay between squares for better visibility
            yield return new WaitForSeconds(0.1f);
        }
        
        isMoving = false;
        
        if (showDebugInfo)
        {
            Debug.Log($"Player reached square {currentSquare}");
        }
    }
    
    // Get current square number
    public int GetCurrentSquare()
    {
        return currentSquare;
    }
    
    // Set player position directly (for initialization or teleporting)
    // squareNumber: 0 = off board, 1-100 = on board
    public void SetPosition(int squareNumber)
    {
        if (boardGenerator == null)
            return;
        
        currentSquare = squareNumber;
        
        // Handle off-board position (0)
        if (squareNumber == 0)
        {
            // Position off board (to the left of square 1)
            Vector3 square1Pos = boardGenerator.GetSquarePosition(1);
            if (square1Pos != Vector3.zero)
            {
                transform.position = new Vector3(square1Pos.x - 2f, square1Pos.y + heightOffset, square1Pos.z);
            }
            return;
        }
        
        if (squareNumber < 1 || squareNumber > 100)
        {
            Debug.LogWarning($"Invalid square number: {squareNumber}. Must be 0 (off board) or 1-100");
            return;
        }
        
        Vector3 pos = boardGenerator.GetSquarePosition(squareNumber);
        if (pos != Vector3.zero)
        {
            transform.position = new Vector3(pos.x, pos.y + heightOffset, pos.z);
        }
    }
    
    // Check if player is currently moving
    public bool IsMoving()
    {
        return isMoving;
    }
}

