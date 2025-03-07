using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    private Board _board;
    
    public Square square;

    public int team;
    public int pieceType = 1;
    
    public SpriteRenderer spriteRenderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _board = Board.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetSquare(Square squareToSet)
    {
        this.square.SetPiece(null);
        this.square = squareToSet;
        this.transform.position = square.transform.position;
        square.SetPiece(this);
    }

    /*
     * Calculates whether a capture from one square to another is valid
     */
    public bool IsValidCapture(Square originalSquare, Square destinationSquare)
    {
        return false;
    }

    /*
     * Attempts to move from one given board location to another
     */
    public abstract bool AttemptMove(Square originalSquare, Square destinationSquare);
    
    public static void CapturePiece(Piece piece)
    {
        Destroy(piece.gameObject);
    }
    
    /*
     * Some checker types can only move forwards
     */
    public static bool FollowsDirectionRule(Piece piece, Square originalSquare, Square destinationSquare)
    {
        
        // normal forward
        if (piece.team == 0 && destinationSquare.coordinates.y > originalSquare.coordinates.y)
            return true;
        
        // warp forward
        if (piece.team == 0 && originalSquare.coordinates.y - destinationSquare.coordinates.y >= (Board.BoardLength.y - 2))
            return true;
        
        // normal backward
        if (piece.team == 1 && destinationSquare.coordinates.y < originalSquare.coordinates.y)
            return true;
        
        // warp backward
        if (piece.team == 1 && destinationSquare.coordinates.y - originalSquare.coordinates.y >= (Board.BoardLength.y - 2))
            return true;

        return false;
    }
}
