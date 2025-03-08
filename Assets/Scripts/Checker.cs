using System.Collections;
using Unity.VisualScripting;
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
        if (Square.IsDirectlyDiagonal(originalSquare, destinationSquare)
            && FollowsDirectionRule(this, originalSquare, destinationSquare)
            && !GameManager.ChainCaptureRunning())
        {
            SetSquare(destinationSquare);
            return true;
        }
        
        // attempt a capture, which may lead into a chain capture
        RunCapture(originalSquare, destinationSquare);

        if (GameManager.ChainCaptureRunning()) return false;
        if (ChainCaptureSuccessful()) return true;
        
        return false;
    }

    private void RunCapture(Square originalSquare, Square destinationSquare)
    {
        // attempt a capture, then keep the chain going if possible. flag captureOccured = true if any happened
        bool captureOccured = AttemptCapture(originalSquare, destinationSquare);
        
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
     * attempt to capture an enemy piece
     * returns true if successful
     */
    private bool AttemptCapture(Square originalSquare, Square destinationSquare)
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
