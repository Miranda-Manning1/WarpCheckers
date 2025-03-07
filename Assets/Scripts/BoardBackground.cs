using System;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Input.GetMouseButtonUp(0) && !GameManager.ClickedOnSquare && Board.SelectedSquare != null)
        {
            Board.SelectedSquare.Deselect();
        }

        GameManager.ClickedOnSquare = false;
    }
}
