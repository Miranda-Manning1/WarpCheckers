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
}
