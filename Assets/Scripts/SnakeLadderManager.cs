using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SnakeLadderManager : MonoBehaviour
{
    [Header("Snakes and Ladders Configuration")]
    [SerializeField] private List<SnakeLadder> snakesAndLadders = new List<SnakeLadder>();
    
    [Header("Visual Settings")]
    [SerializeField] private bool showVisualIndicators = true;
    [SerializeField] private Material ladderMaterial;
    [SerializeField] private Material snakeMaterial;
    [SerializeField] private Color ladderColor = Color.green;
    [SerializeField] private Color snakeColor = Color.red;
    
    [Header("Line Renderer Settings")]
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float lineHeight = 2f; // Height of the line above the board
    
    private Dictionary<int, SnakeLadder> squareMap = new Dictionary<int, SnakeLadder>();
    private SnakeLadderBoardGenerator boardGenerator;
    private List<GameObject> visualIndicators = new List<GameObject>();
    
    private void Start()
    {
        boardGenerator = FindFirstObjectByType<SnakeLadderBoardGenerator>();
        InitializeSnakesAndLadders();
    }
    
    [ContextMenu("Initialize Snakes and Ladders")]
    public void InitializeSnakesAndLadders()
    {
        // Clear existing map
        squareMap.Clear();
        
        // Validate and add to map
        foreach (SnakeLadder sl in snakesAndLadders)
        {
            if (sl.IsValid())
            {
                if (!squareMap.ContainsKey(sl.startSquare))
                {
                    squareMap[sl.startSquare] = sl;
                }
                else
                {
                    Debug.LogWarning($"Square {sl.startSquare} already has a snake/ladder! Skipping duplicate.");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid {sl.name}: Start={sl.startSquare}, End={sl.endSquare}. {(sl.isLadder ? "Ladder must go up" : "Snake must go down")}");
            }
        }
        
        if (showVisualIndicators && boardGenerator != null && boardGenerator.IsBoardGenerated())
        {
            CreateVisualIndicators();
        }
        
        Debug.Log($"Initialized {snakesAndLadders.Count} snakes and ladders. {squareMap.Count} valid entries.");
    }
    
    // Check if a square has a snake or ladder
    public bool HasSnakeOrLadder(int squareNumber)
    {
        return squareMap.ContainsKey(squareNumber);
    }
    
    // Get the destination square if player lands on a snake/ladder
    public int GetDestination(int squareNumber)
    {
        if (squareMap.ContainsKey(squareNumber))
        {
            return squareMap[squareNumber].endSquare;
        }
        return squareNumber; // No snake/ladder, stay on same square
    }
    
    // Get snake/ladder info
    public SnakeLadder GetSnakeLadder(int squareNumber)
    {
        if (squareMap.ContainsKey(squareNumber))
        {
            return squareMap[squareNumber];
        }
        return null;
    }
    
    // Create visual indicators (lines connecting start and end squares)
    private void CreateVisualIndicators()
    {
        // Clear existing indicators
        ClearVisualIndicators();
        
        foreach (SnakeLadder sl in squareMap.Values)
        {
            Vector3 startPos = boardGenerator.GetSquarePosition(sl.startSquare);
            Vector3 endPos = boardGenerator.GetSquarePosition(sl.endSquare);
            
            if (startPos != Vector3.zero && endPos != Vector3.zero)
            {
                CreateLineIndicator(startPos, endPos, sl.isLadder);
            }
        }
    }
    
    private void CreateLineIndicator(Vector3 start, Vector3 end, bool isLadder)
    {
        GameObject lineObject = new GameObject(isLadder ? $"Ladder_{squareMap.Count}" : $"Snake_{squareMap.Count}");
        lineObject.transform.SetParent(transform);
        
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
        
        // Configure line renderer
        lineRenderer.material = isLadder ? 
            (ladderMaterial != null ? ladderMaterial : CreateDefaultMaterial(ladderColor)) :
            (snakeMaterial != null ? snakeMaterial : CreateDefaultMaterial(snakeColor));
        
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        
        // Set positions with height offset
        start.y += lineHeight;
        end.y += lineHeight;
        
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        
        visualIndicators.Add(lineObject);
    }
    
    private Material CreateDefaultMaterial(Color color)
    {
        // Try URP shader first, fallback to standard
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        
        Material mat = new Material(shader);
        mat.color = color;
        return mat;
    }
    
    private void ClearVisualIndicators()
    {
        foreach (GameObject indicator in visualIndicators)
        {
            if (indicator != null)
            {
                DestroyImmediate(indicator);
            }
        }
        visualIndicators.Clear();
    }
    
    // Add a snake or ladder programmatically
    public void AddSnakeLadder(int startSquare, int endSquare, bool isLadder)
    {
        SnakeLadder sl = new SnakeLadder(startSquare, endSquare, isLadder);
        if (sl.IsValid())
        {
            snakesAndLadders.Add(sl);
            InitializeSnakesAndLadders();
        }
        else
        {
            Debug.LogWarning($"Invalid snake/ladder: Start={startSquare}, End={endSquare}");
        }
    }
    
    // Get all snakes and ladders
    public List<SnakeLadder> GetAllSnakesAndLadders()
    {
        return new List<SnakeLadder>(snakesAndLadders);
    }
    
    [ContextMenu("Create Traditional Snakes and Ladders")]
    public void CreateTraditionalSnakesAndLadders()
    {
        snakesAndLadders.Clear();
        
        // Traditional Ladders (go up)
        AddSnakeLadder(2, 23, true);   // Ladder from 2 to 23
        AddSnakeLadder(8, 34, true);   // Ladder from 8 to 34
        AddSnakeLadder(20, 77, true);  // Ladder from 20 to 77
        AddSnakeLadder(32, 68, true);  // Ladder from 32 to 68
        AddSnakeLadder(41, 79, true);  // Ladder from 41 to 79
        AddSnakeLadder(74, 88, true);  // Ladder from 74 to 88
        AddSnakeLadder(82, 100, true); // Ladder from 82 to 100
        
        // Traditional Snakes (go down)
        AddSnakeLadder(29, 9, false);  // Snake from 29 to 9
        AddSnakeLadder(38, 15, false);  // Snake from 38 to 15
        AddSnakeLadder(47, 5, false);   // Snake from 47 to 5
        AddSnakeLadder(53, 33, false);  // Snake from 53 to 33
        AddSnakeLadder(62, 37, false);  // Snake from 62 to 37
        AddSnakeLadder(86, 54, false);  // Snake from 86 to 54
        AddSnakeLadder(92, 70, false);  // Snake from 92 to 70
        AddSnakeLadder(97, 25, false);  // Snake from 97 to 25
        
        InitializeSnakesAndLadders();
        Debug.Log("Created traditional snakes and ladders configuration!");
    }
    
    [ContextMenu("Clear All Snakes and Ladders")]
    public void ClearAllSnakesAndLadders()
    {
        snakesAndLadders.Clear();
        squareMap.Clear();
        ClearVisualIndicators();
        Debug.Log("Cleared all snakes and ladders.");
    }
}

