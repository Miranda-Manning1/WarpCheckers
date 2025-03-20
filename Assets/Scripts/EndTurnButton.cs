using UnityEngine;

public class EndTurnButton : Button
{
    public static EndTurnButton Instance;
    
    void OnMouseUp()
    {
		GameManager.ClickedOnSquare = true;
        Square selectedSquare = GameManager.Board.selectedSquare;
        Square.FinishMove(selectedSquare.GetPiece(), selectedSquare);
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetBoard(Board.Instance);
        gameObject.SetActive(false);
    }
    
}
