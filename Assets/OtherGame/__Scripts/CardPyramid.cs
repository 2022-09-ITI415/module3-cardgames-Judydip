using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum eNewCardState //eNum defines a type of variable that only has a few possible named values- drawpile, tableau, piletop, and discard.
{
    drawpile,
    tableau,
    piletop,
    discard
}

public class CardPyramid : Card
{

    [Header("Set Dynamically: CardPyramid")]

    public eNewCardState state = eNewCardState.drawpile;

    public List<CardPyramid> hiddenBy = new List<CardPyramid>(); //hiddenBy list stores which other cards will keep this face down

    public int layoutID;

    public SlotDef slotDef; //SlotDef class stores info pulled in from the LayoutXML <slot>

    public void Start()
    {

    }
    public override void OnMouseUpAsButton() //allows card to react to being clicked
    {
        Pyramid.S.CardClicked(this);         //call the CardClicked Method on the Prospector singleton
        base.OnMouseUpAsButton();       //also call the base class (Card.cs) version of this method
    }
}
