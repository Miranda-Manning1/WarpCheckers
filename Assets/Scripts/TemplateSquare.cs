using System;
using Unity.VisualScripting;
using UnityEngine;

public class TemplateSquare : Square
{
    public static TemplateSquare Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalColor = Color.black;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
