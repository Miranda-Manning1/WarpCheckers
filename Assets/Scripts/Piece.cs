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

    public void SetSquare(Square square)
    {
        this.square.SetPiece(null);
        this.square = square;
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
}
