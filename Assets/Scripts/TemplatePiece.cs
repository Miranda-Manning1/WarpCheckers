using UnityEngine;

public class TemplatePiece : Piece
{
    public static TemplatePiece Instance;

    void Awake()
    {
        Instance = this;
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        team = -1;
        SetBoard(Board.Instance);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
