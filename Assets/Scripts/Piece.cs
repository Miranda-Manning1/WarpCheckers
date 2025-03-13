using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    private Board _board;
	public GameManager gameManager;
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

    private void SetSquare(Square squareToSet)
    {
        this.square.SetPiece(null);
        this.square = squareToSet;
        SnapToSquare();
		this.transform.SetParent(this.square.transform);
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
     * Capture a piece
     */
    private static void CapturePiece(Piece piece)
    {
		DestroyPiece(piece);
    }

	 /*
     * Destroys a piece, for any reason
     */
	public static void DestroyPiece(Piece piece)
	{
        /*
         * Destroy() marks gameobjects for destruction, which then only happens at the end of the frame,
         * so to prevent issues for the rest of this frame this flag is changed now
         */
        piece.square.SetPiece(null);
        
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

    public void SetChainCaptureSuccessful(bool wasSuccessful)
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

        // failsafe: if no piece currently selected, or if already-selected square is clicked, end attempted movement
        if (!Board.SquareSelected() || originalSquare == destinationSquare) return false;

        // attempt basic movement; set moveSuccessful to true if it's successful
        if (!GameManager.ChainCaptureRunning()
            && AttemptBasicMovement(originalSquare, destinationSquare))
            moveSuccessful = true;

        // attempt special moves; set moveSuccessful to true if any of them are successful
        if (!GameManager.ChainCaptureRunning()
            && !moveSuccessful
            && AttemptSpecialMoves(originalSquare, destinationSquare)) moveSuccessful = true;

		// attempt king-to-queen promotion
		if(!GameManager.ChainCaptureRunning()
			&& !moveSuccessful
			&& AttemptQueenPromotion(originalSquare, destinationSquare)) moveSuccessful = true;

        // attempt a capture, which may lead into a chain capture
        if (!moveSuccessful) AttemptCapture(originalSquare, destinationSquare);

        // avoid ending the turn if there's an ongoing chain capture, or end it if a chain capture has finished
        if (GameManager.ChainCaptureRunning()) avoidEndingTurn = true;
        if (!moveSuccessful && !avoidEndingTurn && ChainCaptureSuccessful()) moveSuccessful = true;
        
        // did the piece move at all, from either a finished turn or a part of a chain capture
        bool didMove = moveSuccessful || avoidEndingTurn;

        if (didMove)
        {
            Board.SquaresTraveledThisTurn.Add(originalSquare);
            AttemptPromotion(originalSquare, destinationSquare);
        }
		if (moveSuccessful) Board.SquaresTraveledThisTurn.Add(destinationSquare);
        
        return moveSuccessful && !avoidEndingTurn;
    }

	/*
	 * attempt stacking one king onto another king to create a queen
	 */
	private bool AttemptQueenPromotion(Square originalSquare, Square destinationSquare) {

		// check all criteria for a king-to-queen promotion
		if (!destinationSquare.IsOccupied()) return false;
		Piece otherPiece = destinationSquare.GetPiece();
		if (otherPiece.pieceType != PieceType.King) return false;
		if (otherPiece.team != this.team) return false;
		if (!Square.IsDiagonallyAdjacent(originalSquare, destinationSquare)) return false;

		// swap pieces and destroy the other piece, effectively replacing the other piece with this one
		Square.SwapSquareContents(originalSquare, destinationSquare);
		DestroyPiece(otherPiece);

		SetPieceType(PieceType.Queen);
		return true;
	}

    private void AttemptPromotion(Square originalSquare, Square destinationSquare)
    {
        /*
         * for a checker to promote to a king, one of the following two must be true:
         * 1) it's on the final square of the board
         * 2) it started on the second-to-last square of the board and ended on the first square of the board (meaning it jumped over the final square)
         */
		if (pieceType == PieceType.Checker && ReachedOppositeSide(originalSquare, destinationSquare))
			this.SetPieceType(PieceType.King);
    }

	/*
	 * set the type of a piece, and change attributes appropriately
	 */
	public void SetPieceType(PieceType newPieceType)
	{
		this.pieceType = newPieceType;

		// set to Checker
		if (newPieceType == PieceType.Checker) {
			directionless = false;
			canSwap = false;
			canCycle = false;
			extraSprite.enabled = false;
			return;
		}

		// set to King
		if (newPieceType == PieceType.King) {
        	directionless = true;
        	canSwap = true;
			extraSprite.sprite = gameManager.spriteArray[1];
			extraSprite.color = Color.red;
        	extraSprite.enabled = true;
			return;
		}

		// set to Queen
		if (newPieceType == PieceType.Queen) {
			directionless = true;
			canSwap = true;
			canCycle = true;
			extraSprite.sprite = gameManager.spriteArray[2];
			extraSprite.color = Color.red;
			extraSprite.enabled = true;
			return;
		}
	}

    /*
     * returns whether the piece moved to or past the opposite end of the board
     */
    private bool ReachedOppositeSide(Square originalSquare, Square destinationSquare)
    {
        switch (team)
        {
            case 0 when IsOnOppositeSide(this, destinationSquare)
                        || (originalSquare.coordinates.y == Board.BoardLength.y - 2 && destinationSquare.coordinates.y == 0):
            case 1 when IsOnOppositeSide(this, destinationSquare)
                        || (originalSquare.coordinates.y == 1 && destinationSquare.coordinates.y == Board.BoardLength.y - 1):
                return true;
            default:
                return false;
        }
    } 
    
    /*
    * returns whether a given square is on a given relative side of the board for a given piece.
    */
    private static bool IsOnRelativeSide(Piece piece, Square square, Board.RelativeSide relativeSide)
    {
        int team = piece.team;
        int y = square.coordinates.y;
        int boardEnd = Board.BoardLength.y - 1;

        switch (relativeSide)
        {
            case Board.RelativeSide.Original:
                return (team == 0) ? (y == 0) : (y == boardEnd);

            case Board.RelativeSide.Opposite:
                return (team == 0) ? (y == boardEnd) : (y == 0);

            default:
                return false;
        }
    }

    /*
     * wrapper method - returns whether a given square is on the original side of the board for a given piece
     */
    private static bool IsOnOriginalSide(Piece piece, Square square)
    {
        return IsOnRelativeSide(piece, square, Board.RelativeSide.Original);
    }
    
    /*
     * wrapper method - returns whether a given square is on the opposite side of the board for a given piece
     */
    private static bool IsOnOppositeSide(Piece piece, Square square)
    {
        return IsOnRelativeSide(piece, square, Board.RelativeSide.Opposite);
    }

    /*
     * attempt a swap move between two pieces
     */
    private bool AttemptSwap(Square thisPieceSquare, Square otherPieceSquare)
    {
        if (!canSwap) return false;
        if (!otherPieceSquare.IsOccupied()) return false;
        if (!Square.IsOrthogonallyAdjacent(thisPieceSquare, otherPieceSquare)) return false;
        
        Piece otherPiece = otherPieceSquare.GetPiece();
        
        // cannot swap if it would warp a piece to its opposite side, resulting in promotion
        if (
            otherPiece.pieceType == PieceType.Checker
            && IsOnOriginalSide(otherPiece, otherPieceSquare)
            && IsOnOppositeSide(otherPiece, thisPieceSquare)
            )
            return false;
        
        Square.SwapSquareContents(thisPieceSquare, otherPieceSquare);
        
        return true;
    }
    
    /*
     * attempts every special move
     */
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
        if (
            !destinationSquare.IsOccupied()
            && Square.IsDiagonallyAdjacent(originalSquare, destinationSquare)
            && FollowsDirectionRule(this, originalSquare, destinationSquare)
            && !GameManager.ChainCaptureRunning()
            )
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

        /*
         * if turning this capture into a chain capture is possible:
         * flag ChainCaptureRunning, then select the destination square
         */
        if (ChainCanContinue(destinationSquare))
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
        // can't move onto an existing piece
        if (destinationSquare.IsOccupied()) return false;
        
        // can only move forwards
        if (!FollowsDirectionRule(this, originalSquare, destinationSquare)) return false;
        
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
    
    /*
     * Sets the piece's visual position to its square
     */
    public void SnapToSquare()
    {
        this.transform.position = this.square.transform.position;
    }
}
