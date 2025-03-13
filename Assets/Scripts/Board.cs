using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using System.Collections.Generic;

public class Board : MonoBehaviour
{

    public static Board Instance;
    public GameManager gameManager;

    public Vector2Int boardLength;
    public static Vector2Int BoardLength = new Vector2Int(8, 8);
    public int CheckerRowsPerSide = 3;
    public static Square[,] Squares;
    public static Square SelectedSquare;

	public static List<Square>[] LastSquaresMoved = new List<Square>[2];
	public static List<Square> SquaresTraveledThisTurn = new List<Square> { };

    private float squareSize = 2f;
    
    public enum RelativeSide
    {
        Original,
        Opposite
    };

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BoardLength = boardLength;
        CheckBoardDimensions();
        
        // partially help square size stay small enough - will likely need a rework later
        if (squareSize * BoardLength.y * 31.5f > Screen.height)
        {
            squareSize = (Screen.height / (float) BoardLength.y) / 31.5f;
        }
        
		List<Square> team0LastSquaresMoved = new List<Square> { };
		List<Square> team1LastSquaresMoved = new List<Square> { };
		LastSquaresMoved[0] = team0LastSquaresMoved;
		LastSquaresMoved[1] = team1LastSquaresMoved;

        CreateSquares();
        CreateCheckers();
        SelectedSquare = TemplateSquare.Instance;
        TemplateSquare.Instance.SetPiece(TemplatePiece.Instance);
    }

    /*
     * ensures that the starting number of checkers per side fit the maximum values
     */
    private void CheckBoardDimensions()
    {
        if (BoardLength.x < 0 || BoardLength.y < 0) BoardLength = new Vector2Int(0, 0);
        if (CheckerRowsPerSide < 0) CheckerRowsPerSide = 0;
        
        int calculatedCheckerRowsPerSide = (BoardLength.y - 1) / 2;
        if (calculatedCheckerRowsPerSide < CheckerRowsPerSide) CheckerRowsPerSide = calculatedCheckerRowsPerSide;
    }

    /*
     * creates the squares on the checkers board
     */
    void CreateSquares()
    {
        Color squareColor1 = Color.black;
        Color squareColor2 = Color.white;

        // initialize list of squares
        Squares = new Square[BoardLength.x, BoardLength.y];

        // grab the template square
        Square templateSquare = TemplateSquare.Instance;

        // prime the size/location of the initial square
        float firstSquareX = transform.position.x - ((BoardLength.x / 2f) * squareSize) + (squareSize / 2);
        float firstSquareY = transform.position.y - ((BoardLength.y / 2f) * squareSize) + (squareSize / 2);
        Vector2 firstSquare = new Vector2(firstSquareX, firstSquareY);

        // create the grid of squares
        int count = 0;
        for (int y = 0; y < BoardLength.y; y++)
        {
            for (int x = 0; x < BoardLength.x; x++)
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
                float xPosition = firstSquare.x + (x * squareSize);
                float yPosition = firstSquare.y + (y * squareSize);
                square.transform.localPosition = new Vector3(xPosition, yPosition, 0);
                square.transform.localScale = new Vector3(squareSize, squareSize, square.transform.localScale.z);


                square.coordinates = new Vector2Int(x, y);
                Squares[x, y] = square;

                count++;
                if (count % BoardLength.x == 0 && BoardLength.x % 2 == 0)
                {
                    (squareColor1, squareColor2) = (squareColor2, squareColor1);
                }
            }
        }
    }

    /*
     * creates a single checkers piece
     */
    private void CreateChecker(int x, int y, int team, int count, Piece templatePiece)
    {
        Square square = Squares[x, y];

        // create piece and set fields
        Piece piece = new GameObject("Piece" + count).AddComponent<Piece>();
        piece.gameManager = FindObjectOfType<GameManager>();
        square.SetPiece(piece);
        piece.transform.position = square.transform.position;
        piece.transform.localScale = new Vector3(squareSize / 2f, squareSize / 2f, piece.transform.localScale.z);
        piece.square = square;
        piece.team = team;
        piece.transform.SetParent(piece.square.transform);

        
        // add sprite gameobjects
        piece.AddComponent<SpriteRenderer>();
        piece.pieceSprite = piece.gameObject.GetComponent<SpriteRenderer>();
        piece.extraSprite = new GameObject("ExtraSprite").AddComponent<SpriteRenderer>();
        
        piece.extraSprite.transform.SetParent(piece.transform);
        
        piece.extraSprite.transform.localPosition = Vector3.zero;

        // disable the extra sprite - will be re-enabled upon piece promotion
        piece.extraSprite.enabled = false;
        
        // set sprites
        piece.pieceSprite.sprite = gameManager.spriteArray[0];
        piece.pieceSprite.sortingOrder = 1;
        piece.extraSprite.sortingOrder = 2;

        if (team == 0)
        {
            piece.pieceSprite.color = Color.cyan;
        }
        
        piece.SetPieceType(Piece.PieceType.Checker);
    }

    /*
     * creates all the starting checkers pieces
     */
    private void CreateCheckers()
    {
        if (CheckerRowsPerSide == 0) return;
        
        Piece templatePiece = TemplatePiece.Instance;

        // Y position of the bottom-left-most checker on teams 0 and 1 respectively
        int[] startY = { 0, BoardLength.y - CheckerRowsPerSide };

        // ending Y position for the checkers being created on teams 0 and 1 respectively
        int[] endY = { CheckerRowsPerSide, BoardLength.y };

        // create two teams of checkers
        int count = 0;
        for (int t = 0; t < 2; t++)
        {
            for (int y = startY[t]; y < endY[t]; y++)
            {
                for (int x = 0; x < BoardLength.x; x++)
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
    public static bool SquareSelected()
    {
        return Board.SelectedSquare != null;
    }

	/*
	 * flips the board for the other team - generally called during turn switching
	 */
    public static void FlipBoard()
    {
        for (int x = 0; x < BoardLength.y; x++)
        {
            for (int y = 0; y < BoardLength.x; y++)
            {
                Square square = Squares[x, y];

                // flip transform position
                square.transform.localPosition = new Vector3(
                    square.transform.localPosition.x * -1,
                    square.transform.localPosition.y * -1, 1);

                // flip coordinates
                square.coordinates = new Vector2Int((
                        Board.BoardLength.x - 1) - square.coordinates.x,
                    (Board.BoardLength.y - 1) - square.coordinates.y);

                // reset piece position to square
                if (square.IsOccupied())
                {
                    square.GetPiece().transform.position = square.transform.position;
                }
            }
        }
    }
}
