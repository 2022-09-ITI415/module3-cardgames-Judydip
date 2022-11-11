using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardColor : Card
{
    // Start is called before the first frame update
    void Start()
    {
        SpriteRenderer spriteColor;
        spriteColor = GetComponent<SpriteRenderer>();
        spriteColor.color = new Color(191, 255, 251, 255);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public override void OnMouseUpAsButton()
    //{

    //}
}
