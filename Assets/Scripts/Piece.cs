using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    private Board _board;
    
    public Square square;

    public int team;
    public int pieceType = 1;
    
    public SpriteRenderer spriteRenderer;
    
    private bool _chainCaptureRunning = false;
    private bool _chainCaptureSuccessful = false;
    
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
        piece.square.SetPiece(null); // Destroy() marks gameobjects for destruction, which then only happens at the end of the frame
        Destroy(piece.gameObject);
    }
    
    /*
     * Some checker types can only move forwards
     */
    public static bool FollowsDirectionRule(Piece piece, Square originalSquare, Square destinationSquare)
    {
        int forwardTeam = 0;
        int backwardTeam = 1;

        if (GameManager.IsBoardFlipped())
        {
            forwardTeam = 1;
            backwardTeam = 0;
        }
        
        // normal forward
        if (piece.team == forwardTeam && destinationSquare.coordinates.y > originalSquare.coordinates.y)
            return true;
        
        // warp forward
        if (piece.team == forwardTeam && originalSquare.coordinates.y - destinationSquare.coordinates.y >= (Board.BoardLength.y - 2))
            return true;
        
        // normal backward
        if (piece.team == backwardTeam && destinationSquare.coordinates.y < originalSquare.coordinates.y)
            return true;
        
        // warp backward
        if (piece.team == backwardTeam && destinationSquare.coordinates.y - originalSquare.coordinates.y >= (Board.BoardLength.y - 2))
            return true;

        return false;
    }
    
    protected bool ChainCaptureSuccessful()
    {
        return _chainCaptureSuccessful;
    }

    protected void SetChainCaptureSuccessful(bool wasSuccessful)
    {
        _chainCaptureSuccessful = wasSuccessful;
    }
}
