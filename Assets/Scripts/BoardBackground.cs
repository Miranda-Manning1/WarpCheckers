using System;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
    private Board _board;

    private void Start()
    {
        _board = GameManager.Board;
    }
    
    private void LateUpdate()
    {
        if (!GameManager.ChainCaptureRunning()
            && Input.GetMouseButtonUp(0)
            && !GameManager.ClickedOnSquare
            && _board.selectedSquare != null)
        {
            _board.selectedSquare.Deselect();
        }

        GameManager.ClickedOnSquare = false;
    }
}