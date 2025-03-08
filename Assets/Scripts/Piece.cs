using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private Board _board;
    public SpriteRenderer pieceSprite;
    public SpriteRenderer extraSprite;
    
    public Square square;

    public int team;

    public enum PieceType
    {
        Checker,
        King,
        Queen,
        Fragment
    };
    
    public PieceType pieceType;
    public bool directionless = false;
    public bool canSwap = false;
    public bool canCycle = false;
    
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

    private static void CapturePiece(Piece piece)
    {
        piece.square.SetPiece(null); // Destroy() marks gameobjects for destruction, which then only happens at the end of the frame
        Destroy(piece.gameObject);
    }
    
    /*
     * Some checker types can only move forwards
     */
    private static bool FollowsDirectionRule(Piece piece, Square originalSquare, Square destinationSquare)
    {
        if (piece.directionless) return true;
        
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
        
        // the below two if statements should only ever be run in dev mode
        
        // normal backward
        if (piece.team == backwardTeam && destinationSquare.coordinates.y < originalSquare.coordinates.y) return true;
        
        // warp backward
        if (piece.team == backwardTeam &&
            destinationSquare.coordinates.y - originalSquare.coordinates.y >= (Board.BoardLength.y - 2)) return true;
        
        return false;
    }
    
    private bool ChainCaptureSuccessful()
    {
        return _chainCaptureSuccessful;
    }

    private void SetChainCaptureSuccessful(bool wasSuccessful)
    {
        _chainCaptureSuccessful = wasSuccessful;
    }
    
    /*
     * attempts to move a checker from point a to point b, returning whether it can do it
     */
    public bool AttemptMove(Square originalSquare, Square destinationSquare)
    {
        bool moveSuccessful = false;
        bool avoidEndingTurn = false;
        
        // can't move onto an existing piece
        if (destinationSquare.IsOccupied()) return false;

        // attempt basic movement; return true if it's successful
        if (AttemptBasicMovement(originalSquare, destinationSquare)) moveSuccessful = true;
        
        if (!moveSuccessful && AttemptSpecialMoves(originalSquare, destinationSquare)) return true;
        
        // attempt a capture, which may lead into a chain capture
        if (!moveSuccessful) AttemptCapture(originalSquare, destinationSquare);
        
        // avoid ending the turn if there's an ongoing chain capture, or end it if a chain capture has finished
        if (GameManager.ChainCaptureRunning()) avoidEndingTurn = true;
        if (!moveSuccessful && !avoidEndingTurn && ChainCaptureSuccessful()) moveSuccessful = true;
        
        AttemptPromotion(originalSquare, destinationSquare);
        
        return moveSuccessful && !avoidEndingTurn;
    }

    public void AttemptPromotion(Square originalSquare, Square destinationSquare)
    {
        /*
         * for a checker piece to promote, one of the following two must be true:
         * 1) it's on the final square of the board
         * 2) it started on the second-to-last square of the board and ended on the first square of the board (meaning it jumped over the final square)
         */
        if (pieceType == PieceType.Checker)
        {
            if (destinationSquare.coordinates.y == Board.BoardLength.y - 1
                || (originalSquare.coordinates.y == Board.BoardLength.y - 2 && destinationSquare.coordinates.y == 0))
            {
                pieceType = PieceType.King;
                directionless = true;
                canSwap = true;
                extraSprite.enabled = true;
            }
        }
    }

    private bool AttemptSwap(Square originalSquare, Square destinationSquare)
    {
        if (!canSwap) return false;
        
        return false;
    }
    
    private bool AttemptSpecialMoves(Square originalSquare, Square destinationSquare)
    {
        if (AttemptSwap(originalSquare, destinationSquare)) return true;
        return false;
    }

    /*
     * attempts a basic movement
     */
    private bool AttemptBasicMovement(Square originalSquare, Square destinationSquare)
    {
        // basic movement
        if (Square.IsDirectlyDiagonal(originalSquare, destinationSquare)
            && FollowsDirectionRule(this, originalSquare, destinationSquare)
            && !GameManager.ChainCaptureRunning())
        {
            SetSquare(destinationSquare);
            return true;
        }

        return false;
    }

    /*
     * runs a capture attempt and handles potential chain captures
     */
    private void AttemptCapture(Square originalSquare, Square destinationSquare)
    {
        // attempt a capture, then keep the chain going if possible. flag captureOccured = true if any happened
        bool captureOccured = AttemptSingleCapture(originalSquare, destinationSquare);
        
        // if capture attempt failed, don't continue to run the capture
        if (!captureOccured) return;

        if (ChainCanContinue(square))
        {
            GameManager.SetChainCaptureRunning(true);
            this.square.Select();
            return;
        }

        GameManager.SetChainCaptureRunning(false);
        SetChainCaptureSuccessful(true);
    }

    /*
     * returns whether a given square is valid for landing on after a capture
     */
    private bool IsValidCaptureDestination(Square originalSquare, Square destinationSquare)
    {
        // can only move forwards
        if (!FollowsDirectionRule(this, originalSquare, destinationSquare)) return false;
        
        // can't move onto an existing piece
        if (destinationSquare.IsOccupied()) return false;
        
        // must land 2 spaces away
        if (!Square.IsDiagonal(originalSquare, destinationSquare, 2)) return false;

        return true;
    }

    /*
     * returns whether a given piece is an option for capturing.
     */
    private bool IsValidCapturePiece(Piece capturePiece)
    {
        // must be a piece to capture
        if (capturePiece == null) return false;
        
        // cannot capture a piece of your own team
        if (capturePiece.team == this.team) return false;

        return true;
    }

    /*
     * returns whether the given capture is a valid option
     */
    private bool IsValidCapture(Square originalSquare, Square destinationSquare, Piece capturePiece)
    {
        if (!IsValidCaptureDestination(originalSquare, destinationSquare)) return false;
        if (!IsValidCapturePiece(capturePiece)) return false;

        return true;
    }

    /*
     * returns whether a capture chain in action can continue
     */
    private bool ChainCanContinue(Square originalSquare)
    {
        int[] directions = { 2, -2 };

        // check all 4 surrounding squares
        foreach (int x in directions)
        {
            foreach (int y in directions)
            {
                Square landingSquare = Square.GetRelativeSquare(square, x, y);
                Square squareBetween = Square.SquareBetween(originalSquare, landingSquare);
                
                // can chain continue
                if (squareBetween.IsOccupied()
                    && IsValidCapture(originalSquare, landingSquare, squareBetween.GetPiece()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*
     * attempt to capture a single enemy piece
     * returns true if successful
     */
    private bool AttemptSingleCapture(Square originalSquare, Square destinationSquare)
    {
        /*
         * check if the clicked square is a valid square for jumping to
         * this is separated from IsValidCapturePiece because we should prevent basic movement during chain captures before we try to grab the capture piece
         */
        if (!IsValidCaptureDestination(originalSquare, destinationSquare)) return false;
        
        Piece capturePiece = Square.PieceBetween(originalSquare, destinationSquare);
        if (!IsValidCapturePiece(capturePiece)) return false;
        
        SetSquare(destinationSquare);
        Piece.CapturePiece(capturePiece);
        
        return true;
    }
}
