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

    void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
    }
    
}
