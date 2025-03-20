using UnityEngine;

public class Button : MonoBehaviour
{
    protected Board _board;
    protected GameManager _gameManager;
    
    /*
     * Set the Board
     */
    public void SetBoard(Board board)
    {
        _board = board;
    }
    
    /*
     * Set the GameManager
     */
    public void SetGameManager(GameManager gameManager)
    {
        _gameManager = gameManager;
    }
}
