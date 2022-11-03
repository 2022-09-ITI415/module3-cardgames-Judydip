using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eCardState //eNum defines a type of variable that only has a few possible named values- drawpile, tableau, target, and discard.
{
    drawpile,
    tableau,
    target,
    discard
}
    public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]

    public eCardState state = eCardState.drawpile; //"This is how you use enum eCardState"

    public List<CardProspector> hiddenBy = new List<CardProspector>(); //hiddenBy list stores which other cards will keep this face down

    public int layoutID; 

    public SlotDef slotDef; //SlotDef class stores info pulled in from the LayoutXML <slot>


    public override void OnMouseUpAsButton() //allows card to react to being clicked
    {
        Prospector.S.CardClicked(this);         //call the CardClicked Method on the Prospector singleton
        base.OnMouseUpAsButton();       //also call the base class (Card.cs) version of this method
    }
}
