using UnityEngine;

public class TemplatePiece : Piece
{
    public static TemplatePiece Instance;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
}
