using System;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
    private Board _board;

    private void Start()
    {
        _board = Board.Instance;
    }

    private void OnMouseUp()
    {
        _board.selectedSquare.Deselect();
    }

    private void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0) && !GameManager.ClickedOnSquare)
        {
            _board.selectedSquare.Deselect();
        }

        GameManager.ClickedOnSquare = false;
    }
}
