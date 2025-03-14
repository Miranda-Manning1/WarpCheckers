using UnityEngine;

public class EndTurnButton : MonoBehaviour
{
    void OnMouseUp()
    {
		GameManager.ClickedOnSquare = true;
        Square selectedSquare = Board.SelectedSquare;
        Square.FinishMove(selectedSquare.GetPiece(), selectedSquare);
    }

    void Start()
    {
        gameObject.SetActive(false);
    }
}
