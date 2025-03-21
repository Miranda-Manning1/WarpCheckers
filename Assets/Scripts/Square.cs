using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using System.Collections.Generic;

public class Square : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D squareCollider;
    
    protected Board _board;
    protected GameManager _gameManager;
    protected CycleButton _cycleButton;
    protected SplitButton _splitButton;

    
    private Piece _occupant;
    public Color originalColor;
    public Vector2Int coordinates;

    void Start()
    {
        _gameManager = GameManager.Instance;
        _board = Board.Instance;
        _cycleButton = CycleButton.Instance;
        _splitButton = SplitButton.Instance;

        if (this == TemplateSquare.Instance)
        {
            originalColor = Color.black;
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    /*
     * Upon selection, highlight the square
     */
    public void Select()
    {
        if (_board.SquareSelected()) _board.selectedSquare.Deselect();
        SetHighlighted(this, true);
        _board.selectedSquare = this;
        
        // if queen selected, enable cycle button
        if (this.GetPiece().CanCycle() && !_gameManager.ChainCaptureRunning() && !_gameManager.SplitRunning())
        {
            _cycleButton.SetCycleEnabled(_board,false);
            _gameManager.SetCycleButtonEnabled(true);
        }
        
        // if king or queen selected, enable split button
        if (this.GetPiece().CanSplit() && !_gameManager.ChainCaptureRunning() && !_gameManager.CycleRunning())
        {
            _splitButton.SetSplitEnabled(_board, false);
            _gameManager.SetSplitButtonEnabled(true);
        }
    }

    /*
     * Deselects when another square is clicked
     */
    public void Deselect()
    {
        _board.selectedSquare = null;
        SetHighlighted(this, false);
        
        // when a piece is deselected, make sure cycle button is turned off
        _cycleButton.SetCycleEnabled(_board, false);
        _gameManager.SetCycleButtonEnabled(false);

        if (!_gameManager.SplitRunning())
        {
            _splitButton.SetSplitEnabled(_board, false);
            _gameManager.SetSplitButtonEnabled(false);
        }
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1) && _gameManager.developerMode && IsOccupied())
        {
            Piece.DestroyPiece(this.GetPiece());
        }

		// press K on a piece when Developer Mode is active to turn it into a King
		if (_gameManager.developerMode && IsOccupied()) {
			if (Input.GetKeyDown(KeyCode.C)) GetPiece().SetPieceType(Piece.PieceType.Checker);
			if (Input.GetKeyDown(KeyCode.K)) GetPiece().SetPieceType(Piece.PieceType.King);
			if (Input.GetKeyDown(KeyCode.Q)) GetPiece().SetPieceType(Piece.PieceType.Queen);
            if (Input.GetKeyDown(KeyCode.T)) GetPiece().SwitchTeam();
        }

        if (_gameManager.developerMode && !IsOccupied())
        {
            if (Input.GetKeyDown(KeyCode.N))
                _board.CreateChecker(this, _gameManager.CurrentPlayerTurn(),
                    Piece.PieceType.Checker);
        }
    }

    /*
     * Selects a square when clicked, and handles piece movement
     */
    private void OnMouseUp()
    {
        _gameManager.clickedOnSquare = true;
        
        // clicking on an empty square when no piece is selected does nothing
        if (!_board.SquareSelected() && !this.IsOccupied()) return;
        
        // clicking on a square that's already selected deselects it
        if (_board.SquareSelected()
            && _board.selectedSquare == this
            && !_gameManager.ChainCaptureRunning()
            && !_gameManager.CycleRunning())
        {
            this.Deselect();
            return;
        }
        
        bool finishedMove = false;
        Piece currentPiece = null;
        
        // when clicking a square, attempt to move the previously selected square's piece to here
        if (_board.SquareSelected()
            && _board.selectedSquare.GetPiece().team == _gameManager.CurrentPlayerTurn())
        {
            currentPiece = _board.selectedSquare.GetPiece();
            finishedMove = currentPiece!.AttemptMove(_board.selectedSquare, this);
        }

        if (finishedMove)
        {
			FinishMove(currentPiece, this);
            return;
        }

        // if a chain capture is currently running, go to next frame
        if (_gameManager.ChainCaptureRunning() || _gameManager.CycleRunning()) {
			return;
		}
        
        // if no move done, just select a square
        if (this.IsOccupied() && this.GetPiece().team == _gameManager.CurrentPlayerTurn())
            Select();
    }

	/*
	 * runs the processes for finishing a move/turn
	 */
	public static void FinishMove(Piece currentPiece, Square finalLocation)
    {
        GameManager gameManager = finalLocation.GetGameManager();
        
		gameManager.SetEndTurnButtonEnabled(false);
        finalLocation._board.squaresTraveledThisTurn.Add(finalLocation);
        finalLocation._board.selectedSquare.Deselect();
		currentPiece.SetChainCaptureSuccessful(false);
        currentPiece.SetCycleSuccessful(false);
        gameManager.SetChainCaptureRunning(false);
        gameManager.SetCycleRunning(false);
		gameManager.SwitchPlayerTurn();
		return;
	}
    
    /*
     * set highlighted or un-highlighted to every square in a List
     */
    public static void SetAllHighlighted(List<Square> squares, bool highlighted)
    {
        foreach (Square square in squares)
        {
            SetHighlighted(square, highlighted);
        }
        return;
    }
    
    /*
     * visually highlight or un-highlight the square
     */
    public static void SetHighlighted(Square square, bool highlighted)
    {
        if (highlighted) square.spriteRenderer.color = (square.originalColor + Color.yellow) / 2;
        if (!highlighted) square.spriteRenderer.color = square.originalColor;
        return;
    }

    public void SetPiece(Piece piece)
    {
        _occupant = piece;
    }

    /*
     * is there a piece on this Square
     */
    public bool IsOccupied()
    {
        return this._occupant != null;
    }

    /*
     * Gets this Square's piece
     */
    public Piece GetPiece()
    {
        return _occupant;
    }
    
    /*
     * Are two squares directly adjacent on the same row or the same column?
     */
    public static bool IsOrthogonallyAdjacent(Square originalSquare, Square destinationSquare)
    {
        return IsOrthogonal(originalSquare, destinationSquare, 1);
    }

    /*
     * Are two squares on either the same row or the same column?
     */
    private static bool IsOrthogonal(Square originalSquare, Square destinationSquare, int spacesApart)
    {
        return IsOnSameColumn(originalSquare, destinationSquare, spacesApart)
               || IsOnSameRow(originalSquare, destinationSquare, spacesApart);
    }

    /*
     * Are two squares on the same row, spacesApart spaces apart?
     */
    private static bool IsOnSameRow(Square originalSquare, Square destinationSquare, int spacesApart)
    {
        Board board = originalSquare._board;
        
        bool yEqual = originalSquare.coordinates.y == destinationSquare.coordinates.y;

        int xCurrentSpacesApart = Math.Abs(originalSquare.coordinates.x - destinationSquare.coordinates.x);
        bool xCorrectSpacesApart = xCurrentSpacesApart == spacesApart
                                   || xCurrentSpacesApart == board.boardLength.x - 1;
        
        return yEqual && xCorrectSpacesApart;
    }

    /*
     * Are two squares on the same column, spacesApart spaces apart?
     */
    private static bool IsOnSameColumn(Square originalSquare, Square destinationSquare, int spacesApart)
    {
        Board board = originalSquare._board;
        
        bool xEqual = originalSquare.coordinates.x == destinationSquare.coordinates.x;
        
        int yCurrentSpacesApart = Math.Abs(originalSquare.coordinates.y - destinationSquare.coordinates.y);
        bool yCorrectSpacesApart = yCurrentSpacesApart == spacesApart || yCurrentSpacesApart == board.boardLength.y - 1;
        
        return xEqual && yCorrectSpacesApart;
    }

    public static bool IsDiagonallyAdjacent(Square originalSquare, Square destinationSquare)
    {
        return IsDiagonal(originalSquare, destinationSquare, 1);
    }

    public static bool IsDiagonal(Square originalSquare, Square destinationSquare, int spacesApart)
    {
        Board board = originalSquare._board;

        // look in both x and y directions
        for (int i = 0; i < 2; i++)
        {
            // check in front of and behind
            if ((originalSquare.coordinates[i] + spacesApart) % board.boardLength[i] == destinationSquare.coordinates[i]
                || (originalSquare.coordinates[i] - spacesApart + board.boardLength[i]) % board.boardLength[i] == destinationSquare.coordinates[i])
                continue;

            // squares aren't diagonal spacesApart spaces apart
            return false;
        }

        // if the code gets to this point, we're all good!
        return true;
    }
    
    /*
     * returns the square that is in between two given squares that are two diagonal spaces apart
     */
    public static Square SquareBetween(Square originalSquare, Square destinationSquare)
    {
        Vector2Int betweenSquareCoordinates = new Vector2Int();
        Board board = originalSquare._board;

        // look in both x and y directions
        for (int i = 0; i < 2; i++)
        {
            // check in front of
            if ((originalSquare.coordinates[i] + 2) % board.boardLength[i] == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] + 1) % board.boardLength[i];
            }
            // check behind
            else if ((originalSquare.coordinates[i] - 2 + board.boardLength[i]) % board.boardLength[i] == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] - 1 + board.boardLength[i]) % board.boardLength[i];
            }
            // squares aren't two apart on the diagonal
            else
            {
                throw new Exception(
                    "Ran Square.SquareBetween() on two squares that were not two spaces apart on the diagonal."
                    );
            }
        }

        /*
         * return the coords of the between square.
         * if the board is flipped, mirror the coordinates to ensure the correct square is returned
         */
        return GetSquareFromCoordinates(board, betweenSquareCoordinates.x, betweenSquareCoordinates.y);
    }

    /*
     * returns the piece that is between two squares 2 apart on the diagonal
     */
    public static Piece PieceBetween(Square originalSquare, Square destinationSquare)
    {
        Square squareBetween = Square.SquareBetween(originalSquare, destinationSquare);
        Piece capturePiece = squareBetween.GetPiece();
        
        return capturePiece;
    }

    /*
     * return a Square based on a set of coordinates, accounting for whether the board is flipped
     */
    public static Square GetSquareFromCoordinates(Board board, int x, int y)
    {
        return board.GetGameManager().IsBoardFlipped() ?
            board.Squares[(board.boardLength.x - 1) - x, (board.boardLength.y - 1) - y]
            : board.Squares[x, y];
    }

    /*
     * returns a Square based on relative coordinates of a given square.
     * given relative coordinates may be negative
     */
    public static Square GetRelativeSquare(Board board, Square originalSquare, int x, int y)
    {
        /*
         * calculate absolute coordinates of square
         * + Board.boardLength.x/y is used to accomodate negative relative coordinates
         */
        Vector2Int absoluteCoordinates = new Vector2Int((
            originalSquare.coordinates.x + x + board.boardLength.x) % board.boardLength.x,
            (originalSquare.coordinates.y + y + board.boardLength.y) % board.boardLength.y);

        return Square.GetSquareFromCoordinates(board, absoluteCoordinates.x, absoluteCoordinates.y);
    }

    /*
     * swap the contents of any two squares
     */
    public static void SwapSquareContents(Square square1, Square square2)
    {
        Square tempSquare = square1;
        Piece tempPiece = square1.GetPiece();
        square1.SetPiece(square2.GetPiece());
        square2.SetPiece(tempPiece);
        if (square1.GetPiece() != null) square1.GetPiece().square = square1;
        if (square2.GetPiece() != null) square2.GetPiece().square = square2;
        
		square1.GetPiece()?.transform.SetParent(square1.transform);
		square2.GetPiece()?.transform.SetParent(square2.transform);
        square1.GetPiece()?.SnapToSquare();
        square2.GetPiece()?.SnapToSquare();
    }

    /*
     * for a List of Squares, can every square be contained in a 2x2 box (warping accomodated)
     */
    private static bool SquaresContainedIn2x2Box(List<Square> squares)
    {
        for (int i = 0; i < squares.Count; i++)
        {
            for (int j = i + 1; j < squares.Count; j++)  // avoid duplicate comparisons
            {
                if (!Square.IsOrthogonallyAdjacent(squares[i], squares[j]) &&
                    !Square.IsDiagonallyAdjacent(squares[i], squares[j]))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /*
     * returns true if 2–3 given squares are valid for cycling so far
     * this method assumes a 90º cycle, and thus that a list of 2 squares is a cycle-in-progress
     */
    public static bool SquaresAreCycleable(List<Square> squares)
    {
        int count = squares.Count;
        if (count == 1) return true;
        if (!SquaresContainedIn2x2Box(squares)) return false;
        
        // for two squares in-progress, they just have to be side-by-side
        if (count == 2 && IsOrthogonallyAdjacent(squares[0], squares[1]))
        {
            return true;
        } else if (count == 2)
        {
            return false;
        }
        
        // for three squares, the second has to be orthogonal to the first, and the third must be diagonal to the first
        if (count == 3
            && IsOrthogonallyAdjacent(squares[0], squares[1])
            && IsDiagonallyAdjacent(squares[0], squares[2]))
        {
            return true;
        } else if (count == 3)
        {
            return false;
        }
        
        throw new Exception(
            "Square.SquaresAreCycleable() ran with Square list count of " + squares.Count
        );
    }

    /*
     * given a List of 3 Squares that can fit inside a 2x2 box, return the 4th missing Square
     */
    public static Square GetMissing2x2Corner(List<Square> squares)
    {
        List<int> xList = new List<int>();
        List<int> yList = new List<int>();

        // get both x values and both y values
        for (int i = 0; i < squares.Count; i++)
        {
            int x = squares[i].coordinates.x;
            int y = squares[i].coordinates.y;
            if (!xList.Contains(x)) xList.Add(x);
            if (!yList.Contains(y)) yList.Add(y);
        }

        // check each of the 4 combinations of coordinates to find which one is missing from the List
        for (int i = 0; i < xList.Count; i++)
        {
            for (int j = 0; j < yList.Count; j++)
            {
                Square squareToCheck = GetSquareFromCoordinates(squares[0]._board, xList[i], yList[j]);
                if (!squares.Contains(squareToCheck)) return squareToCheck;
            }
        }
        
        throw new Exception(
            "Couldn't find missing Square in Square.GetMissing2x2Corner()."
        );
    }
    
    /*
     * cycle the locations of a List of Squares (may create error with too small a List)
     */
    public static void CycleSquareContents(List<Square> squares)
    {
        for (int i = squares.Count - 1; i > 0; i--)
        {
            Square.SwapSquareContents(squares[i], squares[i - 1]);
        }
    }

    /*
     * returns the number of pieces within a given List of Squares
     */
    public static int PiecesInSquareList(List<Square> squares)
    {
        int count = 0;
        for (int i = 0; i < squares.Count; i++)
        {
            if (squares[i].IsOccupied()) count++;
        }

        return count;
    }

    /*
     * Set the square's Board
     */
    public void SetBoard(Board board)
    {
        _board = board;
    }

    /*
     * return the square's board
     */
    public Board GetBoard()
    {
        return _board;
    }

    /*
     * return the square's GameManager
     */
    public GameManager GetGameManager()
    {
        return _gameManager;
    }
}