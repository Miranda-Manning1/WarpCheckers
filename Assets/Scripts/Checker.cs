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
        if (destinationSquare.occupant != null)
        {
            return false;
        }
        
        if (Square.IsDirectlyDiagonal(originalSquare, destinationSquare))
        {
            SetSquare(destinationSquare);
        }
        
        return true;
    }
}
