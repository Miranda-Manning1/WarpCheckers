using UnityEngine;

public class TemplatePiece : Piece
{
    public static TemplatePiece Instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        team = -1;
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
