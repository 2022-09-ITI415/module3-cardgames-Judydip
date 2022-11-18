using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pyramid : MonoBehaviour
{
    static public Pyramid S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    //public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    //public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    //public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    //public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    //public float reloadDelay = 2f;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardPyramid> drawPile;
    public Transform layoutAnchor;
    public CardPyramid piletop;
    public List<CardPyramid> tableau;
    public List<CardPyramid> discardPile;
    //public FloatingScore fsRun;

    CardPyramid firstCard;
    CardPyramid secondCard;
    CardPyramid unselect;

    void Awake()
    {
        S = this; //"set up singleton for Pyramid"

    }
        void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>(); //get Layout component
        layout.ReadLayout(layoutXML.text); //Pass LayoutXML
        drawPile = ConvertListCardsToListCardPyramid(deck.cards);

        LayoutGame();
    }

    void Update()
    {
        if (firstCard == true)
        {
            SpriteRenderer spriteColor;
            spriteColor = firstCard.GetComponentInChildren<SpriteRenderer>();
            spriteColor.color = Color.cyan;
            unselect = firstCard;
        }

        if (firstCard == false && unselect == true)
        {
            SpriteRenderer spriteColor;
            spriteColor = unselect.GetComponentInChildren<SpriteRenderer>();
            spriteColor.color = Color.white;
        }
    }


    List<CardPyramid> ConvertListCardsToListCardPyramid(List<Card> lCD) //Also a class
    {
        List<CardPyramid> lCP = new List<CardPyramid>();
        CardPyramid tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardPyramid;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    CardPyramid Draw()
    {
        CardPyramid cd = drawPile[0];
        drawPile.RemoveAt(0);
        return (cd);
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)           //Creates an empty GameObject to serve as an anchor for the tableau
        {
            GameObject tGO = new GameObject("_LayoutAnchor");       //Create empty GameObject named _LayoutAnchor in Hierarchy
            layoutAnchor = tGO.transform;           //Get its Transform
            layoutAnchor.transform.position = layoutCenter;     //Position it
        }
        CardPyramid cp; //follow the layout

        foreach (SlotDef tSD in layout.slotDefs) //iterate through all SlotDefs in the layout.slotDefs as tSD
        {
            cp = Draw();    //Pull a card from top of drawpile
            cp.faceUp = tSD.faceUp; //Set its faceUp to the value in SlotDef
            cp.transform.parent = layoutAnchor;     //Make its parent layoutAnchor (replaces previous parent: deck.deckAnchor, or _Deck in hierarchy when playing
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            //Sets localPosition of card based on slotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eNewCardState.tableau;
            //Cardpyramids in the tableau have the state CardState.tableau
            cp.SetSortingLayerName(tSD.layerName); //Set the sorting layers

            tableau.Add(cp);    //Add this Cardpyramid to the List<> tableau
        }

        foreach (CardPyramid tCP in tableau)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)   //for each card in the list of hiddenby
            {
                cp = FindCardByLayoutID(hid);       //cp is the id of the card hiding the other card
                tCP.hiddenBy.Add(cp);           //add the card's id to the hiddenby list for tcp
            }
        }

        MoveToPiletop(Draw()); //Set up the initial target card

        UpdateDrawPile();
    }

    CardPyramid FindCardByLayoutID(int layoutID)
    {
        foreach (CardPyramid tCP in tableau)
        {
            //search through all cards in tableau list
            if (tCP.layoutID == layoutID)
            {
                //if the card has the same ID, return it
                return (tCP);
            }
        }
        return (null);
    }

    void MoveToDiscard(CardPyramid cd)   //Moves the current target to the discardPile
    {
        cd.state = eNewCardState.discard;  //Set the state of the card to discard
        discardPile.Add(cd);            //Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor;     //Update its transform parent

        //Position this card on the discardPile
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true; //place it on top of the pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    void MoveToPiletop(CardPyramid cd)  
    {
        if (piletop != null) MoveToDiscard(piletop);
        piletop = cd;        
        cd.state = eNewCardState.piletop;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        cd.faceUp = true; //Make it face-up
                          //Set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //Arranges all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardPyramid cd;
        //Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * layout.drawPile.x,
                layout.multiplier.y * layout.drawPile.y,
                -layout.drawPile.layerID + 0.1f);

            cd.faceUp = false;
            //Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    //What's the easiest way to check if a card is a king, and then move it to the discard pile in each state where the card is clickable?
    //Okay, what cards are clickable and what aren't? Clickable: drawpile, piletop, row 0 and row 1

    public int card_cleared_count;

    public void CardClicked(CardPyramid cd)
    {
        //Trying to get card to change color when clicked

        switch (cd.state)
        {
            case eNewCardState.piletop:
                print("This card is on the piletop");
                if (cd.rank == 13){
                    MoveToDiscard(piletop);
                    card_cleared_count += 1;
                    MoveToPiletop(Draw());
                    UpdateDrawPile();
                }
                if (firstCard == true){
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    print("This card has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        card_cleared_count += 1;
                        firstCard.layoutID = 0; //change the card's sorting order... but I know it's not layoutID.
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        card_cleared_count += 1;
                        secondCard = null;
                        MoveToPiletop(Draw());
                        UpdateDrawPile();
                    } else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                } else
                {
                    firstCard = cd;
                    print("This card has been selected as card one.");
                }
                
                //code it to allow being selected and to glow
                //cd.

                break;

            case eNewCardState.drawpile:
                //Clicking any card in the drawPile will draw the next card
                MoveToDiscard(piletop);
                MoveToPiletop(Draw());
                UpdateDrawPile();
                break;

            case eNewCardState.tableau:
                bool noGo = true;
                foreach (CardPyramid cover in cd.hiddenBy)
                {
                    if (cover.state == eNewCardState.tableau)
                    {
                        print("I am hidden by: " + cover);
                        noGo = false;
                    }
                }
                if (noGo == false)  //if the card is covered, then noGo is false and the case returns before the statements, etc., below it can run.
                {
                    return;
                }

                if (cd.rank == 13)
                {
                    MoveToDiscard(cd);
                    card_cleared_count += 1;
                    if (piletop == null)
                    {
                        MoveToPiletop(Draw());
                    }
                }


                if (firstCard == true)
                {
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    print("This card has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        card_cleared_count += 1;
                        firstCard.SetSortOrder(-10 * firstCard.rank); //maybe? nah
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        card_cleared_count += 1;
                        secondCard.SetSortOrder(-10 * secondCard.rank); //maybe? nah
                        secondCard = null;
                        if (piletop == null){
                            MoveToPiletop(Draw());
                        }
                    } else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                }
                else
                {
                    firstCard = cd;
                    print("This card has been selected as card one.");
                }
               
                //if (EqualsThirteen(piletop, cd))
                //{
                //    MoveToDiscard(piletop);
                //    MoveToDiscard(cd);
                //    MoveToPiletop(Draw());
                //    UpdateDrawPile();
                //}

                //tableau.Remove(cd); //Remove it from the tableau List
                //MoveToPiletop(cd); //Make it the target card
                //SetTableauCanClick();
                break;
        }
    }

    public bool EqualsThirteen(CardPyramid c0, CardPyramid c1)  //return true if the two cards are adjacent in rank (A & K adjacent)
    {
        //if (!c0.faceUp || !c1.faceUp) return (false);

        if (Mathf.Abs(c0.rank + c1.rank) == 13)
        {
            return (true);
        }

        //if (c0.rank == 1) return (true);
        //if (c0.rank == 13) return (true);

        //otherwise return false; page 682, end 704
        return (false);
    }
}
