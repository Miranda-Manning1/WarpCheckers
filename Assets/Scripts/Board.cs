using System;
using Unity.VisualScripting;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class Board : MonoBehaviour
{

    public static Board Instance;
    private GameManager _gameManager;
    
    public TemplateSquare templateSquare;
    public TemplatePiece templatePiece;

    public Vector2Int boardLength = new Vector2Int(8, 8);
    public int checkerRowsPerSide = 3;
    public Square[,] Squares;
    public Square selectedSquare;
    public int pieceCount = 0;

	public List<Square>[] lastSquaresMoved = new List<Square>[2];
	public List<Square> squaresTraveledThisTurn = new List<Square> { };

	public List<Square> cycleSquares = new List<Square> { };

    public float squareSize = 2f;
    
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
    private void Start()
    {
        _gameManager = GameManager.Instance;
        _gameManager.board = this;
        templateSquare = TemplateSquare.Instance;
        templatePiece = TemplatePiece.Instance;
        
        
        CheckBoardDimensions();
        
        // partially help square size stay small enough - will likely need a rework later
        if (squareSize * boardLength.y * 31.5f > Screen.height)
        {
            squareSize = (Screen.height / (float) boardLength.y) / 31.5f;
        }
        
		List<Square> team0LastSquaresMoved = new List<Square> { };
		List<Square> team1LastSquaresMoved = new List<Square> { };
		lastSquaresMoved[0] = team0LastSquaresMoved;
		lastSquaresMoved[1] = team1LastSquaresMoved;

        CreateSquares();
        CreateCheckers();
        selectedSquare = TemplateSquare.Instance;
        TemplateSquare.Instance.SetPiece(TemplatePiece.Instance);
    }

    /*
     * ensures that the starting number of checkers per side fit the maximum values
     */
    private void CheckBoardDimensions()
    {
        if (boardLength.x < 0 || boardLength.y < 0) boardLength = new Vector2Int(0, 0);
        if (checkerRowsPerSide < 0) checkerRowsPerSide = 0;
        
        int calculatedCheckerRowsPerSide = (boardLength.y - 1) / 2;
        if (calculatedCheckerRowsPerSide < checkerRowsPerSide) checkerRowsPerSide = calculatedCheckerRowsPerSide;
    }

    /*
     * creates the squares on the checkers board
     */
    private void CreateSquares()
    {
        Color squareColor1 = Color.black;
        Color squareColor2 = Color.white;

        // initialize list of squares
        Squares = new Square[boardLength.x, boardLength.y];

        // grab the template square
        Square templateSquare = TemplateSquare.Instance;

        // prime the size/location of the initial square
        float firstSquareX = transform.position.x - ((boardLength.x / 2f) * squareSize) + (squareSize / 2);
        float firstSquareY = transform.position.y - ((boardLength.y / 2f) * squareSize) + (squareSize / 2);
        Vector2 firstSquare = new Vector2(firstSquareX, firstSquareY);

        // create the grid of squares
        int count = 0;
        for (int y = 0; y < boardLength.y; y++)
        {
            for (int x = 0; x < boardLength.x; x++)
            {
                // create square and set sprite based on template square's sprite
                Square square = new GameObject("Square" + count).AddComponent<Square>();
                square.transform.SetParent(this.transform);
                square.SetBoard(this);

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
                if (count % boardLength.x == 0 && boardLength.x % 2 == 0)
                {
                    (squareColor1, squareColor2) = (squareColor2, squareColor1);
                }
            }
        }
    }
    
    /*
     * creates a single checkers piece
     */
    public void CreateChecker(Square square, int team, Piece.PieceType pieceType)
    {
        // create piece and set fields
        Piece piece = new GameObject("Piece" + this.pieceCount).AddComponent<Piece>();
        square.SetPiece(piece);
        piece.transform.position = square.transform.position;
        piece.transform.localScale = new Vector3(squareSize / 2f, squareSize / 2f, piece.transform.localScale.z);
        piece.square = square;
        piece.team = team;
        piece.transform.SetParent(piece.square.transform);
        piece.SetBoard(this);
        
        // add sprite gameobjects
        piece.AddComponent<SpriteRenderer>();
        piece.pieceSprite = piece.gameObject.GetComponent<SpriteRenderer>();
        piece.extraSprite = new GameObject("ExtraSprite").AddComponent<SpriteRenderer>();
        
        piece.extraSprite.transform.SetParent(piece.transform);
        
        piece.extraSprite.transform.localPosition = Vector3.zero;

        // disable the extra sprite - will be re-enabled upon piece promotion
        piece.extraSprite.enabled = false;
        
        // set sprites
        piece.pieceSprite.sprite = Piece.CheckerSprite;
        piece.pieceSprite.sortingOrder = 1;
        piece.extraSprite.sortingOrder = 2;

        piece.pieceSprite.color = _gameManager.teamColors[team];
        
        piece.SetPieceType(pieceType);
        
        pieceCount++;
    }

    /*
     * creates all the starting checkers pieces
     */
    private void CreateCheckers()
    {
        if (checkerRowsPerSide == 0) return;
        
        Piece templatePiece = TemplatePiece.Instance;

        // Y position of the bottom-left-most checker on teams 0 and 1 respectively
        int[] startY = { 0, boardLength.y - checkerRowsPerSide };

        // ending Y position for the checkers being created on teams 0 and 1 respectively
        int[] endY = { checkerRowsPerSide, boardLength.y };

        // create two teams of checkers
        for (int t = 0; t < 2; t++)
        {
            for (int y = startY[t]; y < endY[t]; y++)
            {
                for (int x = 0; x < boardLength.x; x++)
                {
                    CreateChecker(Squares[x, y], t, Piece.PieceType.Checker);
                }
            }
        }
    }

    /*
     * Returns whether there is a piece on the selected square
     */
    public bool SquareSelected()
    {
        return selectedSquare != null;
    }

	/*
	 * flips the board for the other team - generally called during turn switching
	 */
    public void FlipBoard()
    {
        for (int x = 0; x < boardLength.y; x++)
        {
            for (int y = 0; y < boardLength.x; y++)
            {
                Square square = Squares[x, y];

                // flip transform position
                square.transform.localPosition = new Vector3(
                    square.transform.localPosition.x * -1,
                    square.transform.localPosition.y * -1, 1);

                // flip coordinates
                square.coordinates = new Vector2Int((
                        boardLength.x - 1) - square.coordinates.x,
                    (boardLength.y - 1) - square.coordinates.y);

                // reset piece position to square
                if (square.IsOccupied())
                {
                    square.GetPiece().transform.position = square.transform.position;
                }
            }
        }
    }

    /*
     * Get the board's gamemanager
     */
    public GameManager GetGameManager()
    {
        return _gameManager;
    }
}
