using UnityEngine;

public class Piece : MonoBehaviour
{

    private GameManager _gameManager;
    private Board _board;
    
    private Vector2 _location;
    public int team = 0;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _gameManager = GameManager.Instance;
        _board = Board.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
