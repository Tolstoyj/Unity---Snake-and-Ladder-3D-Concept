using UnityEngine;

public class SnakeLadderBoardGenerator : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private int boardSize = 10; // 10x10 grid for 100 squares
    [SerializeField] private float squareSize = 1f; // Size of each square
    [SerializeField] private float spacing = 0.1f; // Spacing between squares
    
    [Header("Square Prefab")]
    [SerializeField] private GameObject squarePrefab; // Optional prefab for squares
    
    [Header("Visual Settings")]
    [SerializeField] private Material squareMaterial;
    [SerializeField] private Color evenSquareColor = Color.white;
    [SerializeField] private Color oddSquareColor = Color.gray;
    
    [Header("Text Settings")]
    [SerializeField] private bool showNumbers = true;
    [SerializeField] private int fontSize = 10;
    [SerializeField] private float textScale = 0.5f;
    [SerializeField] private Color textColor = Color.black;
    
    private GameObject[,] boardSquares;
    private int totalSquares = 100;
    
    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        // Clear existing board if any
        ClearBoard();
        
        boardSquares = new GameObject[boardSize, boardSize];
        
        // Calculate total board dimensions
        float totalWidth = (boardSize * squareSize) + ((boardSize - 1) * spacing);
        float totalHeight = (boardSize * squareSize) + ((boardSize - 1) * spacing);
        
        // Start position (bottom-left corner)
        float startX = -totalWidth / 2f + squareSize / 2f;
        float startY = -totalHeight / 2f + squareSize / 2f;
        
        int currentNumber = 1;
        
        // Generate squares in zigzag pattern (traditional Snake and Ladder pattern)
        for (int row = 0; row < boardSize; row++)
        {
            bool isEvenRow = (row % 2 == 0);
            
            for (int col = 0; col < boardSize; col++)
            {
                // Calculate position based on zigzag pattern
                int actualCol = isEvenRow ? col : (boardSize - 1 - col);
                
                float xPos = startX + (actualCol * (squareSize + spacing));
                float yPos = startY + (row * (squareSize + spacing));
                
                Vector3 position = new Vector3(xPos, 0f, yPos);
                
                // Create square
                GameObject square = CreateSquare(currentNumber, position, row, actualCol);
                square.transform.SetParent(transform);
                
                boardSquares[row, actualCol] = square;
                currentNumber++;
            }
        }
        
        Debug.Log($"Snake and Ladder board generated with {totalSquares} squares!");
    }
    
    private GameObject CreateSquare(int number, Vector3 position, int row, int col)
    {
        GameObject square;
        
        // Use prefab if available, otherwise create primitive
        if (squarePrefab != null)
        {
            square = Instantiate(squarePrefab, position, Quaternion.identity);
        }
        else
        {
            // Create a cube as the square
            square = GameObject.CreatePrimitive(PrimitiveType.Cube);
            square.transform.position = position;
            square.transform.localScale = new Vector3(squareSize, 0.1f, squareSize);
            
            // Apply material/color
            Renderer renderer = square.GetComponent<Renderer>();
            if (squareMaterial != null)
            {
                renderer.material = squareMaterial;
            }
            else
            {
                // Alternate colors for checkerboard pattern
                bool isEven = (row + col) % 2 == 0;
                renderer.material.color = isEven ? evenSquareColor : oddSquareColor;
            }
        }
        
        square.name = $"Square_{number}";
        
        // Add number text if enabled
        if (showNumbers)
        {
            AddNumberText(square, number);
        }
        
        // Add a component to store square data
        SquareData squareData = square.AddComponent<SquareData>();
        squareData.squareNumber = number;
        squareData.row = row;
        squareData.col = col;
        
        return square;
    }
    
    private void AddNumberText(GameObject square, int number)
    {
        // Create a child GameObject for text
        GameObject textObject = new GameObject("NumberText");
        textObject.transform.SetParent(square.transform);
        textObject.transform.localPosition = new Vector3(0, 0.06f, 0);
        textObject.transform.localRotation = Quaternion.Euler(90, 0, 0);
        textObject.transform.localScale = new Vector3(textScale, textScale, textScale);
        
        // Add TextMesh component
        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = number.ToString();
        textMesh.fontSize = fontSize;
        textMesh.characterSize = 0.1f; // Smaller character size
        textMesh.color = textColor;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
    }
    
    [ContextMenu("Clear Board")]
    public void ClearBoard()
    {
        // Destroy all child objects
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
        
        boardSquares = null;
        Debug.Log("Board cleared!");
    }
    
    [ContextMenu("Fix Missing Scripts")]
    public void FixMissingScripts()
    {
        int fixedCount = 0;
        
        // Iterate through all child squares
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject square = transform.GetChild(i).gameObject;
            
            // Check if SquareData component is missing
            SquareData squareData = square.GetComponent<SquareData>();
            if (squareData == null)
            {
                // Try to get square number from name
                string squareName = square.name;
                if (squareName.StartsWith("Square_"))
                {
                    string numberStr = squareName.Substring(7);
                    if (int.TryParse(numberStr, out int squareNumber))
                    {
                        // Add SquareData component
                        squareData = square.AddComponent<SquareData>();
                        squareData.squareNumber = squareNumber;
                        
                        // Calculate row and col
                        int row = (squareNumber - 1) / boardSize;
                        int col = (squareNumber - 1) % boardSize;
                        
                        // Adjust for zigzag pattern
                        if (row % 2 != 0)
                        {
                            col = boardSize - 1 - col;
                        }
                        
                        squareData.row = row;
                        squareData.col = col;
                        fixedCount++;
                    }
                }
            }
        }
        
        // Rebuild the array after fixing
        if (fixedCount > 0)
        {
            RebuildBoardArray();
            Debug.Log($"Fixed {fixedCount} missing SquareData components!");
        }
        else
        {
            Debug.Log("No missing scripts found. All squares have SquareData components.");
        }
    }
    
    // Check if board is generated
    public bool IsBoardGenerated()
    {
        // Check if we have the array reference
        if (boardSquares != null && boardSquares.Length > 0)
        {
            return true;
        }
        
        // Fallback: Check if we have child squares (for when board was generated but array lost reference)
        if (transform.childCount >= 100)
        {
            // Try to rebuild the array from children
            RebuildBoardArray();
            return boardSquares != null && boardSquares.Length > 0;
        }
        
        return false;
    }
    
    // Rebuild the boardSquares array from existing child objects
    private void RebuildBoardArray()
    {
        boardSquares = new GameObject[boardSize, boardSize];
        int squaresFound = 0;
        
        // Iterate through all child squares
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject square = transform.GetChild(i).gameObject;
            SquareData squareData = square.GetComponent<SquareData>();
            
            int row = -1;
            int col = -1;
            
            if (squareData != null)
            {
                // Use SquareData if available
                row = squareData.row;
                col = squareData.col;
            }
            else
            {
                // Fallback: Parse square number from name (e.g., "Square_42")
                string squareName = square.name;
                if (squareName.StartsWith("Square_"))
                {
                    string numberStr = squareName.Substring(7); // Skip "Square_"
                    if (int.TryParse(numberStr, out int squareNumber))
                    {
                        row = (squareNumber - 1) / boardSize;
                        col = (squareNumber - 1) % boardSize;
                        
                        // Adjust for zigzag pattern
                        if (row % 2 != 0)
                        {
                            col = boardSize - 1 - col;
                        }
                    }
                }
            }
            
            if (row >= 0 && row < boardSize && col >= 0 && col < boardSize)
            {
                boardSquares[row, col] = square;
                squaresFound++;
            }
        }
        
        Debug.Log($"Board array rebuilt from existing squares. Found {squaresFound} squares.");
    }
    
    // Get square position by number (1-100)
    public Vector3 GetSquarePosition(int squareNumber)
    {
        if (!IsBoardGenerated())
        {
            Debug.LogError("Board has not been generated yet! Please generate the board first using 'Generate Board' context menu.");
            return Vector3.zero;
        }
        
        if (squareNumber < 1 || squareNumber > totalSquares)
        {
            Debug.LogWarning($"Invalid square number: {squareNumber}. Must be between 1 and {totalSquares}");
            return Vector3.zero;
        }
        
        int row = (squareNumber - 1) / boardSize;
        int col = (squareNumber - 1) % boardSize;
        
        // Adjust for zigzag pattern
        if (row % 2 != 0)
        {
            col = boardSize - 1 - col;
        }
        
        if (row < boardSize && col < boardSize && boardSquares != null && boardSquares[row, col] != null)
        {
            return boardSquares[row, col].transform.position;
        }
        
        // Fallback: Try to find square by name
        string squareName = $"Square_{squareNumber}";
        Transform squareTransform = transform.Find(squareName);
        if (squareTransform != null)
        {
            return squareTransform.position;
        }
        
        Debug.LogWarning($"Square {squareNumber} not found at row {row}, col {col}");
        return Vector3.zero;
    }
    
    // Get square GameObject by number
    public GameObject GetSquare(int squareNumber)
    {
        if (squareNumber < 1 || squareNumber > totalSquares)
        {
            return null;
        }
        
        int row = (squareNumber - 1) / boardSize;
        int col = (squareNumber - 1) % boardSize;
        
        // Adjust for zigzag pattern
        if (row % 2 != 0)
        {
            col = boardSize - 1 - col;
        }
        
        if (boardSquares != null && row < boardSize && col < boardSize)
        {
            return boardSquares[row, col];
        }
        
        return null;
    }
}

// Helper class to store square data
public class SquareData : MonoBehaviour
{
    public int squareNumber;
    public int row;
    public int col;
}

