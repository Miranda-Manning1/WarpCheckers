using System;
using NUnit.Framework.Constraints;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * Upon selection, attempt a move and highlight
     */
    public void Select()
    {
        _board.selectedSquare.Deselect();
        spriteRenderer.color = (originalColor + Color.yellow) / 2;
        _board.selectedSquare = this;
        _isSelected = true;
        GameManager.ClickedOnSquare = true;
    }

    /*
     * Deselects when another square is clicked
     */
    public void Deselect()
    {
        _isSelected = false;
        spriteRenderer.color = originalColor;
    }

    /*
     * Selects a square when clicked
     */
    private void OnMouseUp()
    {
        bool isValidMove = false;
        // when clicking a square, attempt to move the previously selected square's piece to here
        if (_board.selectedSquare != null && _board.selectedSquare._occupant != null)
            isValidMove = _board.selectedSquare._occupant.AttemptMove(_board.selectedSquare, this);
        
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

        return Board.Squares[betweenSquareCoordinates.x, betweenSquareCoordinates.y];
    }
}
