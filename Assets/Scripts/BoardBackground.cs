using System;
using UnityEngine;

public class BoardBackground : MonoBehaviour
{
    private Board _board;
    private GameManager _gameManager;

    private void Start()
    {
        _board = Board.Instance;
        _gameManager = GameManager.Instance;
    }
    
    private void LateUpdate()
    {
        if (!_gameManager.ChainCaptureRunning()
            && Input.GetMouseButtonUp(0)
            && !_gameManager.clickedOnSquare
            && _board.selectedSquare != null)
        {
            _board.selectedSquare.Deselect();
        }

        _gameManager.clickedOnSquare = false;
    }
}