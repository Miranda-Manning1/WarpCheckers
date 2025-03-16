using UnityEngine;

public class Button : MonoBehaviour
{
    protected Board _board;
    
    /*
     * Set the Board
     */
    public void SetBoard(Board board)
    {
        _board = board;
    }
}
