using UnityEngine;
using UnityEngine.InputSystem;

public class DiceGenerator : MonoBehaviour
{
    [Header("Dice Settings")]
    [SerializeField] private float diceSize = 1f;
    [SerializeField] private Material diceMaterial;
    [SerializeField] private Color diceColor = Color.white;
    
    [Header("Dot Settings")]
    [SerializeField] private bool showDots = true;
    [SerializeField] private float dotSize = 0.15f;
    [SerializeField] private Material dotMaterial;
    [SerializeField] private Color dotColor = Color.black;
    [SerializeField] private float dotOffset = 0.01f; // How far dots are from the face
    [SerializeField] private float dotSpacing = 0.3f; // Spacing between dots on a face
    
    [Header("Dice Prefab")]
    [SerializeField] private GameObject dicePrefab; // Optional prefab for dice
    
    [Header("Click Settings")]
    [SerializeField] private bool enableClickToRoll = true;
    [SerializeField] private float rollAnimationDuration = 1f;
    
    [Header("Game Integration")]
    public GameManager gameManager;
    
    public GameObject diceObject; // Made public so GameManager can check if it exists
    private bool isRolling = false;
    
    // Event for dice roll
    public System.Action<int> OnDiceRollComplete;
    
    private void Start()
    {
        // Find game manager if not assigned
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("DiceGenerator: Found GameManager automatically!");
            }
        }
    }
    
    // Face directions in local space (before rotation)
    // These correspond to: Front(1), Back(6), Right(5), Left(2), Top(3), Bottom(4)
    private readonly Vector3[] faceDirections = new Vector3[]
    {
        Vector3.forward,   // Front - 1
        Vector3.back,      // Back - 6
        Vector3.right,     // Right - 5
        Vector3.left,      // Left - 2
        Vector3.up,        // Top - 3
        Vector3.down       // Bottom - 4
    };
    
    private readonly int[] faceValues = new int[] { 1, 6, 5, 2, 3, 4 };
    
    [ContextMenu("Generate Dice")]
    public void GenerateDice()
    {
        // Clear existing dice if any
        ClearDice();
        
        // Create dice
        if (dicePrefab != null)
        {
            diceObject = Instantiate(dicePrefab, transform.position, transform.rotation);
            diceObject.transform.SetParent(transform);
        }
        else
        {
            // Create a cube as the dice
            diceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            diceObject.transform.SetParent(transform);
            diceObject.transform.localPosition = Vector3.zero;
            diceObject.transform.localRotation = Quaternion.identity;
            diceObject.transform.localScale = Vector3.one * diceSize;
            
            // Apply material/color
            Renderer renderer = diceObject.GetComponent<Renderer>();
            if (diceMaterial != null)
            {
                renderer.material = diceMaterial;
            }
            else
            {
                renderer.material.color = diceColor;
            }
        }
        
        diceObject.name = "Dice";
        
        // Ensure collider exists for click detection
        Collider diceCollider = diceObject.GetComponent<Collider>();
        if (diceCollider == null)
        {
            diceCollider = diceObject.AddComponent<BoxCollider>();
        }
        
        // Add dice data component
        DiceData diceData = diceObject.AddComponent<DiceData>();
        diceData.diceGenerator = this;
        
        // Add click handler component
        if (enableClickToRoll)
        {
            DiceClickHandler clickHandler = diceObject.AddComponent<DiceClickHandler>();
            clickHandler.diceGenerator = this;
        }
        
        // Add dots to faces if enabled
        if (showDots)
        {
            AddDiceDots();
        }
        
        Debug.Log("Dice generated successfully!");
    }
    
    private void AddDiceDots()
    {
        // Traditional dice face arrangement:
        // Front (Z+): 1
        // Back (Z-): 6
        // Right (X+): 5
        // Left (X-): 2
        // Top (Y+): 3
        // Bottom (Y-): 4
        
        // Front face (Z+) - 1 dot
        CreateFaceDots(Vector3.forward, Vector3.up, Vector3.right, 1, "Front");
        
        // Back face (Z-) - 6 dots
        CreateFaceDots(Vector3.back, Vector3.up, Vector3.left, 6, "Back");
        
        // Right face (X+) - 5 dots
        CreateFaceDots(Vector3.right, Vector3.up, Vector3.back, 5, "Right");
        
        // Left face (X-) - 2 dots
        CreateFaceDots(Vector3.left, Vector3.up, Vector3.forward, 2, "Left");
        
        // Top face (Y+) - 3 dots
        CreateFaceDots(Vector3.up, Vector3.forward, Vector3.right, 3, "Top");
        
        // Bottom face (Y-) - 4 dots
        CreateFaceDots(Vector3.down, Vector3.forward, Vector3.right, 4, "Bottom");
    }
    
    private void CreateFaceDots(Vector3 faceDirection, Vector3 upDirection, Vector3 rightDirection, int dotCount, string faceName)
    {
        float offset = (diceSize / 2f) + dotOffset;
        Vector3 faceCenter = faceDirection * offset;
        
        // Calculate the spacing based on dice size
        float spacing = dotSpacing * diceSize;
        
        // Get dot positions for this number
        Vector3[] dotPositions = GetDotPositions(dotCount, spacing);
        
        // Create dots
        for (int i = 0; i < dotPositions.Length; i++)
        {
            // Transform local positions to world space relative to the face
            Vector3 localPos = dotPositions[i];
            Vector3 worldPos = faceCenter + (rightDirection * localPos.x) + (upDirection * localPos.y);
            
            CreateDot(worldPos, faceDirection, $"{faceName}_Dot_{i + 1}");
        }
    }
    
    private Vector3[] GetDotPositions(int count, float spacing)
    {
        Vector3[] positions = new Vector3[count];
        
        switch (count)
        {
            case 1: // Center dot
                positions[0] = Vector3.zero;
                break;
                
            case 2: // Diagonal dots (top-left to bottom-right)
                positions[0] = new Vector3(-spacing * 0.5f, spacing * 0.5f, 0);
                positions[1] = new Vector3(spacing * 0.5f, -spacing * 0.5f, 0);
                break;
                
            case 3: // Diagonal dots
                positions[0] = new Vector3(-spacing * 0.5f, spacing * 0.5f, 0);
                positions[1] = Vector3.zero;
                positions[2] = new Vector3(spacing * 0.5f, -spacing * 0.5f, 0);
                break;
                
            case 4: // Four corners
                positions[0] = new Vector3(-spacing * 0.5f, spacing * 0.5f, 0);
                positions[1] = new Vector3(spacing * 0.5f, spacing * 0.5f, 0);
                positions[2] = new Vector3(-spacing * 0.5f, -spacing * 0.5f, 0);
                positions[3] = new Vector3(spacing * 0.5f, -spacing * 0.5f, 0);
                break;
                
            case 5: // Four corners + center
                positions[0] = new Vector3(-spacing * 0.5f, spacing * 0.5f, 0);
                positions[1] = new Vector3(spacing * 0.5f, spacing * 0.5f, 0);
                positions[2] = Vector3.zero; // Center
                positions[3] = new Vector3(-spacing * 0.5f, -spacing * 0.5f, 0);
                positions[4] = new Vector3(spacing * 0.5f, -spacing * 0.5f, 0);
                break;
                
            case 6: // Two columns of three
                positions[0] = new Vector3(-spacing * 0.5f, spacing * 0.5f, 0);
                positions[1] = new Vector3(-spacing * 0.5f, 0, 0);
                positions[2] = new Vector3(-spacing * 0.5f, -spacing * 0.5f, 0);
                positions[3] = new Vector3(spacing * 0.5f, spacing * 0.5f, 0);
                positions[4] = new Vector3(spacing * 0.5f, 0, 0);
                positions[5] = new Vector3(spacing * 0.5f, -spacing * 0.5f, 0);
                break;
        }
        
        return positions;
    }
    
    private void CreateDot(Vector3 position, Vector3 faceDirection, string name)
    {
        // Create a sphere as the dot
        GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dot.name = name;
        dot.transform.SetParent(diceObject.transform);
        dot.transform.localPosition = position;
        dot.transform.localScale = Vector3.one * dotSize;
        
        // Apply material/color
        Renderer renderer = dot.GetComponent<Renderer>();
        if (dotMaterial != null)
        {
            renderer.material = dotMaterial;
        }
        else
        {
            renderer.material.color = dotColor;
        }
        
        // Remove collider (optional, to avoid physics issues)
        Collider collider = dot.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyImmediate(collider);
        }
    }
    
    [ContextMenu("Clear Dice")]
    public void ClearDice()
    {
        if (diceObject != null)
        {
            DestroyImmediate(diceObject);
            diceObject = null;
        }
        
        Debug.Log("Dice cleared!");
    }
    
    [ContextMenu("Fix Click Handler")]
    public void FixClickHandler()
    {
        if (diceObject == null)
        {
            Debug.LogWarning("Dice object not found! Please generate dice first.");
            return;
        }
        
        // Remove existing click handler if any
        DiceClickHandler existingHandler = diceObject.GetComponent<DiceClickHandler>();
        if (existingHandler != null)
        {
            DestroyImmediate(existingHandler);
        }
        
        // Add new click handler
        if (enableClickToRoll)
        {
            DiceClickHandler clickHandler = diceObject.AddComponent<DiceClickHandler>();
            clickHandler.diceGenerator = this;
            Debug.Log("Click handler added to dice!");
        }
        else
        {
            Debug.LogWarning("Click to roll is disabled. Enable it in the Inspector first.");
        }
        
        // Ensure collider exists
        Collider collider = diceObject.GetComponent<Collider>();
        if (collider == null)
        {
            diceObject.AddComponent<BoxCollider>();
            Debug.Log("Collider added to dice!");
        }
    }
    
    // Handle dice click - called by DiceClickHandler
    public void OnDiceClicked()
    {
        if (isRolling)
        {
            Debug.LogWarning("Dice is already rolling! Please wait.");
            return;
        }
        
        if (diceObject == null)
        {
            Debug.LogError("Dice object is null! Cannot roll.");
            return;
        }
        
        Debug.Log("Dice clicked!");
        
        // If game manager exists and game is active, use game manager's roll method
        if (gameManager != null && gameManager.IsGameActive() && gameManager.IsWaitingForDiceRoll())
        {
            Debug.Log("Using GameManager to roll dice...");
            gameManager.RollDiceForCurrentPlayer();
        }
        else
        {
            // Otherwise, just roll the dice directly
            Debug.Log("Rolling dice directly (no game manager or game not active)...");
            RollDiceAnimated(rollAnimationDuration);
        }
    }
    
    // Roll the dice (random rotation)
    public void RollDice()
    {
        if (diceObject != null)
        {
            // Random rotation on all axes
            Vector3 randomRotation = new Vector3(
                Random.Range(0f, 360f),
                Random.Range(0f, 360f),
                Random.Range(0f, 360f)
            );
            diceObject.transform.rotation = Quaternion.Euler(randomRotation);
            
            // Get and display the dice value
            int value = GetDiceValue();
            Debug.Log($"Dice rolled! Value: {value}");
        }
    }
    
    // Get current face value by detecting which face is pointing up
    public int GetDiceValue()
    {
        if (diceObject == null)
            return Random.Range(1, 7);
        
        // World up direction
        Vector3 worldUp = Vector3.up;
        
        float maxDot = float.MinValue;
        int topFaceIndex = 0;
        
        // Check which local face direction (after rotation) is closest to world up
        for (int i = 0; i < faceDirections.Length; i++)
        {
            // Transform local direction to world direction
            Vector3 worldDirection = diceObject.transform.TransformDirection(faceDirections[i]);
            
            // Dot product with world up (1.0 = pointing up, -1.0 = pointing down)
            float dot = Vector3.Dot(worldDirection, worldUp);
            
            if (dot > maxDot)
            {
                maxDot = dot;
                topFaceIndex = i;
            }
        }
        
        return faceValues[topFaceIndex];
    }
    
    // Animate dice roll
    public void RollDiceAnimated(float duration = 1f)
    {
        if (diceObject == null)
        {
            Debug.LogError("Dice object is null! Cannot roll. Please generate dice first.");
            return;
        }
        
        if (isRolling)
        {
            Debug.LogWarning("Dice is already rolling! Please wait.");
            return;
        }
        
        Debug.Log("Starting dice roll animation...");
        StartCoroutine(RollDiceCoroutine(duration));
    }
    
    private System.Collections.IEnumerator RollDiceCoroutine(float duration)
    {
        isRolling = true;
        
        float elapsed = 0f;
        Vector3 startRotation = diceObject.transform.rotation.eulerAngles;
        Vector3 endRotation = startRotation + new Vector3(
            Random.Range(360f, 720f),
            Random.Range(360f, 720f),
            Random.Range(360f, 720f)
        );
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Ease out curve
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            diceObject.transform.rotation = Quaternion.Euler(Vector3.Lerp(startRotation, endRotation, t));
            yield return null;
        }
        
        diceObject.transform.rotation = Quaternion.Euler(endRotation);
        isRolling = false;
        
        // Get and display the dice value after rolling
        int value = GetDiceValue();
        Debug.Log($"Dice rolled! Value: {value}");
        
        // Notify game manager
        if (gameManager != null)
        {
            Debug.Log($"Notifying GameManager of dice roll: {value}");
            gameManager.HandleDiceRoll(value);
        }
        else
        {
            Debug.LogWarning("GameManager is null! Dice roll result not processed.");
        }
        
        // Invoke event
        OnDiceRollComplete?.Invoke(value);
    }
}

// Helper class to store dice data
public class DiceData : MonoBehaviour
{
    public DiceGenerator diceGenerator;
}

// Helper class to handle dice clicks
public class DiceClickHandler : MonoBehaviour
{
    public DiceGenerator diceGenerator;
    private bool initialized = false;
    
    private void Start()
    {
        Initialize();
    }
    
    private void Initialize()
    {
        if (initialized) return;
        
        // Find dice generator if not assigned
        if (diceGenerator == null)
        {
            diceGenerator = GetComponentInParent<DiceGenerator>();
            if (diceGenerator == null)
            {
                diceGenerator = FindFirstObjectByType<DiceGenerator>();
            }
        }
        
        if (diceGenerator == null)
        {
            Debug.LogError("DiceClickHandler: DiceGenerator not found!");
        }
        else
        {
            Debug.Log("DiceClickHandler: DiceGenerator found and connected!");
        }
        
        // Ensure collider exists
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning("DiceClickHandler: No collider found! Adding BoxCollider...");
            gameObject.AddComponent<BoxCollider>();
        }
        
        initialized = true;
    }
    
    private void Update()
    {
        if (!initialized)
        {
            Initialize();
        }
        
        // Check for mouse click (left or right button) using raycast
        Mouse mouse = Mouse.current;
        if (mouse == null)
            return;
        
        bool leftClick = mouse.leftButton.wasPressedThisFrame;
        bool rightClick = mouse.rightButton.wasPressedThisFrame;
        
        if (leftClick || rightClick)
        {
            // Raycast from camera to mouse position
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = FindFirstObjectByType<Camera>();
            }
            
            if (cam != null)
            {
                Vector2 mousePos = mouse.position.ReadValue();
                Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
                RaycastHit hit;
                
                // Use a longer max distance and check all layers
                if (Physics.Raycast(ray, out hit, 1000f))
                {
                    // Check if we clicked on this dice or any of its children/parents
                    Transform hitTransform = hit.collider.transform;
                    Transform myTransform = transform;
                    
                    bool isDice = (hit.collider.gameObject == gameObject) ||
                                   hitTransform.IsChildOf(myTransform) ||
                                   myTransform.IsChildOf(hitTransform) ||
                                   hitTransform.root == myTransform.root;
                    
                    if (isDice)
                    {
                        Debug.Log($"Dice clicked! Hit: {hit.collider.name}, GameObject: {gameObject.name}");
                        if (diceGenerator != null)
                        {
                            diceGenerator.OnDiceClicked();
                        }
                        else
                        {
                            Debug.LogError("DiceGenerator is null in DiceClickHandler!");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No camera found for dice click detection!");
            }
        }
    }
}

