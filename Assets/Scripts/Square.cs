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
        Board.SelectedSquare.Deselect();
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
        spriteRenderer.color = originalColor;
    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1) && GameManager.DeveloperMode && IsOccupied())
        {
            Destroy(this.GetPiece().gameObject);
        }
    }

    /*
     * Selects a square when clicked, and handles piece movement
     */
    private void OnMouseUp()
    {
        GameManager.ClickedOnSquare = true;
        
        // clicking on piece of non-current-player does nothing
        if (this.IsOccupied() && this.GetPiece().team != GameManager.CurrentPlayerTurn()) return;
        
        // clicking on an empty square when no piece is selected does nothing
        if (!Board.PieceSelected() && !this.IsOccupied()) return;
        
        bool finishedMove = false;
        Piece currentPiece = null;
        
        // when clicking a square, attempt to move the previously selected square's piece to here
        if (Board.PieceSelected())
        {
            currentPiece = Board.SelectedSquare.GetPiece();
            finishedMove = currentPiece!.AttemptMove(Board.SelectedSquare, this);
        }

        if (finishedMove)
        {
            Board.SelectedSquare.Deselect();
            GameManager.ClickedOnSquare = true;
            GameManager.SwitchPlayerTurn();
            return;
        }

        // if a chain capture is currently running, go to next frame
        if (currentPiece != null && GameManager.ChainCaptureRunning()) return;
        
        // if no move done, just select a square
        if (this.IsOccupied())
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

    public static bool IsDirectlyDiagonal(Square originalSquare, Square destinationSquare)
    {
        return IsDiagonal(originalSquare, destinationSquare, 1);
    }

    public static bool IsDiagonal(Square originalSquare, Square destinationSquare, int spacesApart)
    {

        // look in both x and y directions
        for (int i = 0; i < 2; i++)
        {
            // check in front of and behind
            if ((originalSquare.coordinates[i] + spacesApart) % 8 == destinationSquare.coordinates[i]
                || (originalSquare.coordinates[i] - spacesApart + 8) % 8 == destinationSquare.coordinates[i])
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
            if ((originalSquare.coordinates[i] + 2) % 8 == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] + 1) % 8;
            }
            // check behind
            else if ((originalSquare.coordinates[i] - 2 + 8) % 8 == destinationSquare.coordinates[i])
            {
                betweenSquareCoordinates[i] = (originalSquare.coordinates[i] - 1 + 8) % 8;
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
         * + 8 is used to accomodate negative relative coordinates
         */
        Vector2Int absoluteCoordinates = new Vector2Int((
            originalSquare.coordinates.x + x + 8) % 8,
            (originalSquare.coordinates.y + y + 8) % 8);

        return Square.GetSquareFromCoordinates(absoluteCoordinates.x, absoluteCoordinates.y);
    }
}
