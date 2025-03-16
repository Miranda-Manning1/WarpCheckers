using System;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    public SpriteRenderer pieceSprite;
    public SpriteRenderer extraSprite;

    public static Sprite CheckerSprite;
    private static Sprite _kingSprite;
    private static Sprite _queenSprite;
    
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
    
    private bool _chainCaptureSuccessful = false;
	private bool _cycleSuccessful = false;

	void Awake()
	{
		CheckerSprite = Resources.Load<Sprite>("Checker");
		_kingSprite = Resources.Load<Sprite>("King");
		_queenSprite = Resources.Load<Sprite>("Queen");
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

    private bool CycleSuccessful()
    {
        return _chainCaptureSuccessful;
    }

    public void SetCycleSuccessful(bool wasSuccessful)
    {
        _cycleSuccessful = wasSuccessful;
    }
    
    /*
     * attempts to move a checker from point a to point b, returning whether it can do it
     */
    public bool AttemptMove(Square originalSquare, Square destinationSquare)
    {
        
        bool moveSuccessful = false; // did a move, and therefore a turn, finish after this click
        bool avoidEndingTurn = false; // regardless of whether a move was finished this click, should the end of the turn be avoided
		bool captureThisClick = false; // did this piece capture an enemy piece in this click?
		bool cycleEnabled = CycleButton.CycleEnabled();

        // failsafe: if no piece currently selected, or if already-selected square is clicked, end attempted movement
        if (!Board.SquareSelected() || originalSquare == destinationSquare) return false;

        // attempt basic movement; set moveSuccessful to true if it's successful
        if (!GameManager.ChainCaptureRunning()
            && !cycleEnabled
			&& AttemptBasicMovement(originalSquare, destinationSquare))
            moveSuccessful = true;

        // attempt special moves; set moveSuccessful to true if any of them are successful
        if (!GameManager.ChainCaptureRunning()
            && !moveSuccessful
            && AttemptSpecialMoves(originalSquare, destinationSquare, cycleEnabled)) moveSuccessful = true;

		// attempt king-to-queen promotion
		if(!GameManager.ChainCaptureRunning()
			&& !GameManager.CycleRunning()
			&& !cycleEnabled
			&& !moveSuccessful
			&& AttemptQueenPromotion(originalSquare, destinationSquare)) moveSuccessful = true;

        // attempt a capture, which may lead into a chain capture
        if (!moveSuccessful
			&& !GameManager.CycleRunning()
			&& !cycleEnabled
			&& AttemptCapture(originalSquare, destinationSquare)) captureThisClick = true;

        /*
		 * avoid ending the turn if there's an ongoing chain capture or cycle
		 * or end it if a chain capture or cycle has finished
		 */
        if (GameManager.ChainCaptureRunning() || GameManager.CycleRunning()) avoidEndingTurn = true;
        if (!moveSuccessful && !avoidEndingTurn
			&& (ChainCaptureSuccessful() || CycleSuccessful())) moveSuccessful = true;
        
        // did the piece move at all, from either a finished turn or a part of a chain capture
        bool didMove = moveSuccessful || captureThisClick;

        if (didMove)
        {
            Board.SquaresTraveledThisTurn.Add(originalSquare);
            AttemptPromotion(originalSquare, destinationSquare);
        }

		// if there's an ongoing chain capture, enable the End Turn button
		if (GameManager.ChainCaptureRunning()) GameManager.SetEndTurnButtonEnabled(true);
        
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
		if (this.pieceType != PieceType.King) return false;
		if (otherPiece.team != this.team) return false;
		if (!Square.IsDiagonallyAdjacent(originalSquare, destinationSquare)) return false;

		PromoteToQueen(originalSquare, destinationSquare);
		return true;
	}

	/*
	 * Given two squares, stack the King on the first square onto the King on the second.
	 * Assumes that all criteria for queen promotion are met
	 */
	private void PromoteToQueen(Square originalSquare, Square destinationSquare)
	{
		Piece otherPiece = destinationSquare.GetPiece();
		
		if (otherPiece == null) throw new Exception(
			"Ran Piece.PromoteToQueen() when destinationSquare's piece was null"
		);
		
		// swap pieces and destroy the other piece, effectively replacing the other piece with this one
		Square.SwapSquareContents(originalSquare, destinationSquare);
		DestroyPiece(otherPiece);

		SetPieceType(PieceType.Queen);
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
	 * set the type of piece, and change attributes appropriately
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
			extraSprite.sprite = _kingSprite;
			extraSprite.color = Color.red;
        	extraSprite.enabled = true;
			return;
		}

		// set to Queen
		if (newPieceType == PieceType.Queen) {
			directionless = true;
			canSwap = true;
			canCycle = true;
			extraSprite.sprite = _queenSprite;
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

        if (team != GameManager.BackwardTeam)
        {
            return IsOnOppositeSide(this, destinationSquare)
                   || (originalSquare.coordinates.y == Board.BoardLength.y - 2 && destinationSquare.coordinates.y == 0);
        }

        if (team == GameManager.BackwardTeam)
        {
            return IsOnOppositeSide(this, destinationSquare)
                   || (originalSquare.coordinates.y == 1 && destinationSquare.coordinates.y == Board.BoardLength.y - 1);
        }

        return false;
    } 
    
    /*
    * returns whether a given square is on a given relative side of the board for a given piece.
    */
    private static bool IsOnRelativeSide(Piece piece, Square square, Board.RelativeSide relativeSide)
    {
        int team = piece.team;
        int y = square.coordinates.y;
        int boardEnd = Board.BoardLength.y - 1;

        /*
         * if team is the backwards team, original side and opposite side are switched from those of the non-backwards team
         */
        return relativeSide switch
        {
	        Board.RelativeSide.Original => (team == GameManager.BackwardTeam) ? (y == boardEnd) : (y == 0),
	        Board.RelativeSide.Opposite => (team == GameManager.BackwardTeam) ? (y == 0) : (y == boardEnd),
	        _ => false
        };
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
        if (WouldBeInstaPromoted(otherPiece, otherPieceSquare, thisPieceSquare)) return false;
        
        Square.SwapSquareContents(thisPieceSquare, otherPieceSquare);
        
        return true;
    }
    
    /*
     * attempts every special move
     */
    private bool AttemptSpecialMoves(Square originalSquare, Square destinationSquare, bool cycleEnabled)
    {
        if (!cycleEnabled && AttemptSwap(originalSquare, destinationSquare)) return true;
		if (cycleEnabled && AttemptCycle(originalSquare, destinationSquare)) return true;
        return false;
    }

	private bool AttemptCycle(Square originalSquare, Square destinationSquare)
	{
		if (Board.CycleSquares.Contains(destinationSquare))
		{
			FailedCycleCleanup();
			return false;
		}
		
		Board.CycleSquares.Add(destinationSquare);
		int size = Board.CycleSquares.Count;

		// for a 180º cycle, only the Queen and a diagonally adjacent square must be clicked
		if (size == 2 && Square.IsDiagonallyAdjacent(originalSquare, destinationSquare)) {

			// fetch the missing Squares and add them to the cycle list
			Square newSquare1 = Square.GetSquareFromCoordinates(this.square.coordinates.x,
				destinationSquare.coordinates.y);
			Square newSquare2 = Square.GetSquareFromCoordinates(destinationSquare.coordinates.x,
				this.square.coordinates.y);
			
			Board.CycleSquares.Add(newSquare1);
			Board.CycleSquares.Add(newSquare2);

			// if there's no piece in there besides a Queen
			if (Square.PiecesInSquareList(Board.CycleSquares) < 2) {
				FailedCycleCleanup();
				return false;
			}

			// cycle cannot result in an insta-promotion
			if ((destinationSquare.IsOccupied() &&
			     WouldBeInstaPromoted(destinationSquare.GetPiece(), destinationSquare, originalSquare))
			    || (newSquare1.IsOccupied() && WouldBeInstaPromoted(newSquare1.GetPiece(), newSquare1, newSquare2))
			    || newSquare2.IsOccupied() && WouldBeInstaPromoted(newSquare2.GetPiece(), newSquare2, newSquare1))
				return FailedCycleCleanup();
			
			// make sure the two previously missing squares are highlighted upon turn switch
			Board.SquaresTraveledThisTurn.Add(newSquare1);
			Board.SquaresTraveledThisTurn.Add(newSquare2);

			// make the 180º cycle
			Square.SwapSquareContents(originalSquare, destinationSquare);
			Square.SwapSquareContents(newSquare1, newSquare2);

			return SuccessfulCycleCleanup();
		}
		
		// 90º cycle
		if (Square.SquaresAreCycleable(Board.CycleSquares)) {

			// if only 2/3 squares selected: set the cycle running flag, highlight the new square, and move on to the next frame
			if (size <= 2) {
				GameManager.SetCycleRunning(true);
				Board.SquaresTraveledThisTurn.Add(destinationSquare);
				Square.SetHighlighted(destinationSquare, true);
				return false;
			}

			// 3rd square selected
			if (size == 3) {
				// fetch the missing Square
				Square missingSquare = Square.GetMissing2x2Corner(Board.CycleSquares);
				Board.CycleSquares.Add(missingSquare);

				// if there's no piece in there besides a Queen
				if (Square.PiecesInSquareList(Board.CycleSquares) < 2) return FailedCycleCleanup();

				// run the cycle, check for insta-promotions, and undo if any occured
				if (!CycleAndCheckForInstaPromotions()) return FailedCycleCleanup();
				
				// make sure the previously missing square is highlighted upon turn switch
				Board.SquaresTraveledThisTurn.Add(missingSquare);
				return SuccessfulCycleCleanup();
			}
		}

		return FailedCycleCleanup();
	}

	/*
	 * cycle Board.CycleSquares, then undo if it's found to cause an insta-promotion
	 */
	private bool CycleAndCheckForInstaPromotions()
	{
		// make a copy of the list of squares, and a list of all those squares' pieces
		List<Square> originalSquares = new List<Square> { };
		List<Piece> originalPieces = new List<Piece> { };
		for (int i = 1; i < Board.CycleSquares.Count; i++)
		{
			Square currentSquare = Board.CycleSquares[i];
			originalSquares.Add(currentSquare);
			originalPieces.Add(null);
			if (currentSquare.IsOccupied()) originalPieces[i - 1] = currentSquare.GetPiece();
		}
		
		// cycle the contents
		Square.CycleSquareContents(Board.CycleSquares);

		// check each square for a piece that may have been insta-promoted. if any found, undo cycle
		for (int i = 0; i < originalPieces.Count; i++)
		{
			if (originalPieces[i] != null &&
			    WouldBeInstaPromoted(originalPieces[i], originalSquares[i], originalPieces[i].square))
			{
				Square.CycleSquareContents(Board.CycleSquares);
				Square.CycleSquareContents(Board.CycleSquares);
				Square.CycleSquareContents(Board.CycleSquares);
				return false;
			}
		}

		return true;
	}

	/*
	 * code to run after a cycle is successfully executed
	 * this will always return true - this is so "return SuccessfulCycleCleanup()" can run the code, then return true
	 */
	private bool SuccessfulCycleCleanup()
	{
		Board.CycleSquares.Clear();
		SetCycleSuccessful(true);
		GameManager.SetCycleRunning(false);

		return true;
	}

	/*
	 * code to run when an attempted cycle fails
	 * this will always return false - this is so "return FailedCycleCleanup()" can run the code, then return false
	 */
	private bool FailedCycleCleanup()
	{
		// un-highlight every cycle square except the initial Queen
		for (int i = 1; i < Board.CycleSquares.Count; i++)
		{
			Square squareToUnHighlight = Board.CycleSquares[i];

			// only un-highlight the square if it wasn't a square moved in the opponent's last turn
			if (!Board.LastSquaresMoved[GameManager.GetOppositeTeam(this.team)].Contains(squareToUnHighlight))
				Square.SetHighlighted(squareToUnHighlight, false);
		}

		GameManager.SetCycleRunning(false);
		Board.SquaresTraveledThisTurn.Clear();
		Board.CycleSquares.Clear();
		Board.CycleSquares.Add(this.square);

		return false;
	}
	
	/*
	 * would a particular piece move from its original side to its opposite side, resulting in an instant promotion
	 */
	private bool WouldBeInstaPromoted(Piece piece, Square originalSquare, Square destinationSquare)
	{
		if (
			piece.pieceType == PieceType.Checker
			&& IsOnOriginalSide(piece, originalSquare)
			&& IsOnOppositeSide(piece, destinationSquare)
		) return true;
		
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
    private bool AttemptCapture(Square originalSquare, Square destinationSquare)
    {
        // attempt a capture, then keep the chain going if possible. flag captureOccured = true if any happened
        bool captureOccured = AttemptSingleCapture(originalSquare, destinationSquare);
        
        // if capture attempt failed, don't continue to run the capture
        if (!captureOccured) return false;

        /*
         * if turning this capture into a chain capture is possible:
         * flag ChainCaptureRunning, then select the destination square
         */
        if (ChainCanContinue(destinationSquare))
        { 
            GameManager.SetChainCaptureRunning(true);
            this.square.Select();
            return true;
        }

        GameManager.SetChainCaptureRunning(false);
        SetChainCaptureSuccessful(true);
		return true;
    }

    /*
     * returns whether a given square is valid for landing on after a capture
     */
    private bool IsValidCaptureDestination(Square originalSquare, Square destinationSquare)
    {
        // can't move onto an existing piece, unless they're both Kings
        if (destinationSquare.IsOccupied()
            && !AreSamePieceTypeOnSameTeam(destinationSquare.GetPiece(), PieceType.King)) return false;
        
        // can only move forwards
        if (!FollowsDirectionRule(this, originalSquare, destinationSquare)) return false;
        
        // must land 2 spaces away
        if (!Square.IsDiagonal(originalSquare, destinationSquare, 2)) return false;

        return true;
    }

    /*
     * are this piece and a given piece both the same piece type on the same team
     */
    private bool AreSamePieceTypeOnSameTeam(Piece piece, PieceType checkPieceType)
    {
	    return this.team == piece.team && this.pieceType == checkPieceType && piece.pieceType == checkPieceType;
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

        // Either jump onto a King of the same team, creating a Queen, or do a regular capture
        if (destinationSquare.IsOccupied())
        {
	        PromoteToQueen(originalSquare, destinationSquare);
        }
        else
        {
	        SetSquare(destinationSquare);
        }
        
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

    public void SwitchTeam()
    {
	    team = GameManager.GetOppositeTeam(team);
	    pieceSprite.color = GameManager.TeamColors[team];
    }
}
