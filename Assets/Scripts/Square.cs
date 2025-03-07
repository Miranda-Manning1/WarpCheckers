using System;
using NUnit.Framework.Constraints;
using UnityEngine;

public class Square : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D squareCollider;
    
    private Board _board;
    public Color originalColor;

    private bool _isSelected = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _board = Board.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Select()
    {
        _board.selectedSquare.Deselect();
        spriteRenderer.color = (originalColor + Color.yellow) / 2;
        _board.selectedSquare = this;
        _isSelected = true;
        GameManager.ClickedOnSquare = true;
    }

    public void Deselect()
    {
        _isSelected = false;
        spriteRenderer.color = originalColor;
    }

    private void OnMouseUp()
    {
        Select();
    }
}
