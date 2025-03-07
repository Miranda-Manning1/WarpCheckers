using UnityEngine;

public class Checker : Piece
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /*
     * attempts to move a checker from point a to point b, returning whether it can do it
     */
    public override bool AttemptMove(Square originalSquare, Square destinationSquare)
    {
        // can't move onto an existing piece
        if (destinationSquare.IsOccupied())
        {
            return false;
        }
        
        // basic movement
        if (Square.IsDirectlyDiagonal(originalSquare, destinationSquare))
        {
            SetSquare(destinationSquare);
            return true;
        }
        
        // capture an opponent's piece
        AttemptCapture(originalSquare, destinationSquare);
        
        return false;
    }

    public bool AttemptCapture(Square originalSquare, Square destinationSquare)
    {
        // can't move onto an existing piece
        if (destinationSquare.IsOccupied()) return false;
        
        // must land 2 spaces away
        if (!Square.IsDiagonal(originalSquare, destinationSquare, 2)) return false;
        
        Square squareBetween = Square.SquareBetween(originalSquare, destinationSquare);
        Piece capturePiece = squareBetween.GetPiece();

        // must be a piece to capture
        if (capturePiece == null) return false;
        
        // cannot capture a piece of your own team
        if (capturePiece.team == this.team) return false;
        
        SetSquare(destinationSquare);
        Piece.CapturePiece(capturePiece);
        return true;
    }
}
