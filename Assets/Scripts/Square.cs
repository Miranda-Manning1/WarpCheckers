using System;
using NUnit.Framework.Constraints;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public class Square : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D squareCollider;
    
    private Board _board;
    private Piece _occupant;
    public Color originalColor;
    public Vector2Int coordinates;

    private bool _isSelected = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _board = Board.Instance;
    }

    /*
     * Upon selection, attempt a move and highlight
     */
    public void Select()
    {
        if (Board.SquareSelected()) Board.SelectedSquare.Deselect();
        spriteRenderer.color = (originalColor + Color.yellow) / 2;
        Board.SelectedSquare = this;
        _isSelected = true;
    }

    /*
     * Deselects when another square is clicked
     */
    public void Deselect()
    {
        _isSelected = false;
        Board.SelectedSquare = null;
        spriteRenderer.color = originalColor;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1) && GameManager.DeveloperMode && IsOccupied())
        {
            Destroy(this.GetPiece().gameObject);
        }

		// press K on a piece when Developer Mode is active to turn it into a King
		if (GameManager.DeveloperMode && IsOccupied()) {
			if (Input.GetKeyDown(KeyCode.K)) GetPiece().SetPieceType(Piece.PieceType.Checker);
			if (Input.GetKeyDown(KeyCode.K)) GetPiece().SetPieceType(Piece.PieceType.King);
			if (Input.GetKeyDown(KeyCode.Q)) GetPiece().SetPieceType(Piece.PieceType.Queen);
		}
    }

    /*
     * Selects a square when clicked, and handles piece movement
     */
    private void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
        
        // clicking on an empty square when no piece is selected does nothing
        if (!Board.SquareSelected() && !this.IsOccupied()) return;
        
        // clicking on a square that's already selected deselects it
        if (Board.SquareSelected() && Board.SelectedSquare == this)
        {
            this.Deselect();
            return;
        }
        
        bool finishedMove = false;
        Piece currentPiece = null;
        
        // when clicking a square, attempt to move the previously selected square's piece to here
        if (Board.SquareSelected() && Board.SelectedSquare.GetPiece().team == GameManager.CurrentPlayerTurn())
        {
            currentPiece = Board.SelectedSquare.GetPiece();
            finishedMove = currentPiece!.AttemptMove(Board.SelectedSquare, this);
        }

        if (finishedMove)
        {
            Board.SelectedSquare.Deselect();
            GameManager.ClickedOnSquare = true;
            currentPiece.SetChainCaptureSuccessful(false);
            GameManager.SwitchPlayerTurn();
            return;
        }

        // if a chain capture is currently running, go to next frame
        if (currentPiece != null && GameManager.ChainCaptureRunning()) return;
        
        // if no move done, just select a square
        if (this.IsOccupied() && this.GetPiece().team == GameManager.CurrentPlayerTurn())
            Select();
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
        bool yEqual = originalSquare.coordinates.y == destinationSquare.coordinates.y;

        int xCurrentSpacesApart = Math.Abs(originalSquare.coordinates.x - destinationSquare.coordinates.x);
        bool xCorrectSpacesApart = xCurrentSpacesApart == spacesApart || xCurrentSpacesApart == Board.BoardLength.x - 1;
        
        return yEqual && xCorrectSpacesApart;
    }

    /*
     * Are two squares on the same column, spacesApart spaces apart?
     */
    private static bool IsOnSameColumn(Square originalSquare, Square destinationSquare, int spacesApart)
    {
        bool xEqual = originalSquare.coordinates.x == destinationSquare.coordinates.x;
        
        int yCurrentSpacesApart = Math.Abs(originalSquare.coordinates.y - destinationSquare.coordinates.y);
        bool yCorrectSpacesApart = yCurrentSpacesApart == spacesApart || yCurrentSpacesApart == Board.BoardLength.y - 1;
        
        return xEqual && yCorrectSpacesApart;
    }

    public static bool IsDiagonallyAdjacent(Square originalSquare, Square destinationSquare)
    {
        return IsDiagonal(originalSquare, destinationSquare, 1);
    }

    public static bool IsDiagonal(Square originalSquare, Square destinationSquare, int spacesApart)
    {

        // look in both x and y directions
        for (int i = 0; i < 2; i++)
        {
            // check in front of and behind
            if ((originalSquare.coordinates[i] + spacesApart) % Board.BoardLength[i] == destinationSquare.coordinates[i]
                || (originalSquare.coordinates[i] - spacesApart + Board.BoardLength[i]) % Board.BoardLength[i] == destinationSquare.coordinates[i])
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

        // look in both x and y directions
        for (int i = 0; i < 2; i++)
        {
            // check in front of
            if ((originalSquare.coordinates[i] + 2) % Board.BoardLength[i] == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] + 1) % Board.BoardLength[i];
            }
            // check behind
            else if ((originalSquare.coordinates[i] - 2 + Board.BoardLength[i]) % Board.BoardLength[i] == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] - 1 + Board.BoardLength[i]) % Board.BoardLength[i];
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
        return GetSquareFromCoordinates(betweenSquareCoordinates.x, betweenSquareCoordinates.y);
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
    public static Square GetSquareFromCoordinates(int x, int y)
    {
        return GameManager.IsBoardFlipped() ?
            Board.Squares[(Board.BoardLength.x - 1) - x, (Board.BoardLength.y - 1) - y]
            : Board.Squares[x, y];
    }

    /*
     * returns a Square based on relative coordinates of a given square.
     * given relative coordinates may be negative
     */
    public static Square GetRelativeSquare(Square originalSquare, int x, int y)
    {
        /*
         * calculate absolute coordinates of square
         * + Board.BoardLength.x/y is used to accomodate negative relative coordinates
         */
        Vector2Int absoluteCoordinates = new Vector2Int((
            originalSquare.coordinates.x + x + Board.BoardLength.x) % Board.BoardLength.x,
            (originalSquare.coordinates.y + y + Board.BoardLength.x) % Board.BoardLength.x);

        return Square.GetSquareFromCoordinates(absoluteCoordinates.x, absoluteCoordinates.y);
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
        square1.GetPiece().square = square1;
        square2.GetPiece().square = square2;
        
		square1.GetPiece().transform.SetParent(square1.transform);
		square2.GetPiece().transform.SetParent(square2.transform);
        square1.GetPiece().SnapToSquare();
        square2.GetPiece().SnapToSquare();
    }
}

/*
 *         Square tempSquare = this.square;
        this.square.SetPiece(destinationSquare.GetPiece());
        destinationSquare.SetPiece(this);
        this.square = destinationSquare;
        destinationSquare.GetPiece().square = tempSquare;
 */
