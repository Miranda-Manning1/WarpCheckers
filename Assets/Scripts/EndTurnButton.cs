using UnityEngine;

public class EndTurnButton : Button
{
    public static EndTurnButton Instance;
    
    void OnMouseUp()
    {
		_gameManager.clickedOnSquare = true;
        Square selectedSquare = _board.selectedSquare;
        Square.FinishMove(selectedSquare.GetPiece(), selectedSquare);
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetBoard(Board.Instance);
        SetGameManager(GameManager.Instance);
        
        gameObject.SetActive(false);
    }
    
}
