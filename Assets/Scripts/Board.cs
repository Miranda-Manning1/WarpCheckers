using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Board : MonoBehaviour
{

    public static Board Instance;
    private Vector2 _boardLength = new Vector2(8, 8); // number of squares
    public static Square[,] Squares;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateSquares();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void CreateSquares()
    {
        Color squareColor1 = Color.white;
        Color squareColor2 = Color.black;
        
        // initialize list of squares
        Squares = new Square[(int)_boardLength.x, (int)_boardLength.y];
        
        // grab the template square
        Square templateSquare = TemplateSquare.Instance;
        
        // prime the size/location of the initial square
        Vector2 squareSize = new Vector2(1120 / (1120 / 2f), 510 / (510 / 2f));
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
                SpriteRenderer spriteRenderer = square.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = templateSquare.GetComponent<SpriteRenderer>().sprite;

                // set square visuals
                spriteRenderer.color = count % 2 == 0 ? squareColor1 : squareColor2;
                spriteRenderer.sortingOrder = 0;
                
                // set square location
                float xPosition = firstSquare.x + (x * squareSize.x);
                float yPosition = firstSquare.y + (y * squareSize.y);
                square.transform.localPosition = new Vector3(xPosition, yPosition, 0);
                square.transform.localScale = new Vector3(Screen.width / (1120 / 0.18f), Screen.height / (510 / 0.18f), 1);

                Squares[x, y] = square;
                
                count++;
                if (count % _boardLength.x == 0) (squareColor1, squareColor2) = (squareColor2, squareColor1);
            }
        }
    }
}
