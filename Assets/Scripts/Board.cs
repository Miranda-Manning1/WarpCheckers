using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class Board : MonoBehaviour
{

    public static Board Instance;
    private Vector2Int _boardLength = new Vector2Int(8, 8); // number of squares
    public static Square[,] Squares;
    public static Square SelectedSquare;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CreateSquares();
        CreateCheckers();
        SelectedSquare = TemplateSquare.Instance;
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
                square.spriteRenderer.sortingOrder = 0;
                
                // set square location
                float xPosition = firstSquare.x + (x * squareSize.x);
                float yPosition = firstSquare.y + (y * squareSize.y);
                square.transform.localPosition = new Vector3(xPosition, yPosition, 0);

                square.coordinates = new Vector2Int(x, y);
                Squares[x, y] = square;
                
                count++;
                if (count % _boardLength.x == 0) (squareColor1, squareColor2) = (squareColor2, squareColor1);
            }
        }
    }

    void CreateChecker(int x, int y, int team, int count, Piece templatePiece)
    {
        Square square = Squares[x, y];
        
        // create piece and set fields
        Checker checker = new GameObject("Piece" + count).AddComponent<Checker>();
        checker.transform.SetParent(this.transform);
        square.SetPiece(checker);
        checker.transform.position = square.transform.position;
        checker.square = square;
        checker.team = team;
        checker.pieceType = 1;
            
        // add components to piece
        checker.spriteRenderer = checker.AddComponent<SpriteRenderer>();
        checker.spriteRenderer.sprite = templatePiece.spriteRenderer.sprite;
        checker.spriteRenderer.sortingOrder = 1;

        if (team == 0)
        {
            checker.spriteRenderer.color = Color.cyan;
        }
    }

    void CreateCheckers()
    {
        Checker templatePiece = TemplatePiece.Instance;
        
        int[] startY = {0, _boardLength.y - 3};
        int[] endY = { 3, _boardLength.y };
        
        // create two teams of checkers
        int count = 0;
        for (int t = 0; t < 2; t++)
        {
            for (int y = startY[t]; y < endY[t]; y++)
            {
                for (int x = 0; x < _boardLength.x; x++)
                {
                    CreateChecker(x, y, t, count, templatePiece);
                    count++;
                }
            }
        }
    }

    /*
     * Returns whether there is a piece on the selected square
     */
    public static bool PieceSelected()
    {
        return Board.SelectedSquare != null && Board.SelectedSquare.GetPiece() != null;
    }
}
