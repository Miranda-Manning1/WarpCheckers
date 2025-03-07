using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Board : MonoBehaviour
{

    public static Board Instance;
    private Vector2 _boardLength = new Vector2(8, 8); // number of squares
    public static Square[,] Squares;
    public Square selectedSquare;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateSquares();
        selectedSquare = TemplateSquare.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateSquares()
    {
        Color squareColor1 = Color.black;
        Color squareColor2 = Color.white;
        
        // initialize list of squares
        Squares = new Square[(int)_boardLength.x, (int)_boardLength.y];
        
        // grab the template square
        Square templateSquare = TemplateSquare.Instance;
        
        // prime the size/location of the initial square
        Vector2 squareSize = new Vector2(Screen.width / (1120 / 2f), Screen.height / (510 / 2f));
        float firstSquareX = transform.position.x - ((_boardLength.x / 2) * squareSize.x) + (squareSize.x / 2);
        float firstSquareY = transform.position.y - ((_boardLength.y / 2) * squareSize.y) + (squareSize.y / 2);
        Vector2 firstSquare = new Vector2(firstSquareX, firstSquareY);
        
        // create the grid of squares
        int count = 0;
        for (int y = 0; y < _boardLength.y; y++)
        {
            for (int x = 0; x < _boardLength.x; x++)
            {
                // create square and set sprite based on template square's sprite
                Square square = new GameObject("Square" + count).AddComponent<Square>();
                square.transform.SetParent(this.transform);
                
                // add components to square
                square.spriteRenderer = square.AddComponent<SpriteRenderer>();
                square.spriteRenderer.sprite = templateSquare.spriteRenderer.sprite;
                square.squareCollider = square.AddComponent<BoxCollider2D>();
                square.squareCollider.layerOverridePriority = 1;

                // set square visuals
                square.originalColor = count % 2 == 0 ? squareColor1 : squareColor2;
                square.spriteRenderer.color = square.originalColor;
                square.spriteRenderer.sortingOrder = 1;
                
                // set square location
                float xPosition = firstSquare.x + (x * squareSize.x);
                float yPosition = firstSquare.y + (y * squareSize.y);
                square.transform.localPosition = new Vector3(xPosition, yPosition, 0);

                Squares[x, y] = square;
                
                count++;
                if (count % _boardLength.x == 0) (squareColor1, squareColor2) = (squareColor2, squareColor1);
            }
        }
    }
}
