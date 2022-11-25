using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

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
    public Text uiClearCount;
    public int cardClearCount;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardPyramid> drawPile;
    public Transform layoutAnchor;
    public List<CardPyramid> piletop;
    public List<CardPyramid> tableau;
    public List<CardPyramid> discardPile;
    //public FloatingScore fsRun;

    CardPyramid firstCard;
    CardPyramid secondCard;
    CardPyramid unselect;
    //CardPyramid belowTarget;
    //CardPyramid notBelow;

    public bool GameOver;
    public int carddrawcount = 1;

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

        cardClearCount = 0;

        UpdateGUI();
        //TestingList();
    }
    void UpdateGUI()
    {
        uiClearCount.text = "You have cleared " + cardClearCount + " cards" ;
    }

    //public void TestingList()
    //{
    //    var forTest = new List<string>();
    //    forTest.Add("one");
    //    forTest.Add("two");
    //    forTest.Add("three");
    //    var lastItem = forTest.Last();
    //    print(lastItem);
    //}

    void Update()
    {
        UpdateGUI();

        //Stuff that makes the cards change color when clicking them.
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

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("GameScene");
        }

        //if (piletop.Count > 1)
        //{
        //    SpriteRenderer spriteColor;
        //    spriteColor = belowTarget.GetComponentInChildren<SpriteRenderer>();
        //    spriteColor.color = Color.gray;
        //    notBelow = belowTarget;
        //}
        //if (belowTarget == false && notBelow == true)
        //{
        //    SpriteRenderer spriteColor;
        //    spriteColor = notBelow.GetComponentInChildren<SpriteRenderer>();
        //    spriteColor.color = Color.white;
        //}
    }


    List<CardPyramid> ConvertListCardsToListCardPyramid(List<Card> lCD)
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
        cd.SetSortingLayerName("Target");
        return (cd);
    }

    void LayoutGame()
    {
        if (layoutAnchor == null)           //Creates an empty GameObject to serve as an anchor for the tableau
        {
            GameObject tGO = new GameObject("_LayoutAnchor");       //Create empty GameObject named _LayoutAnchor in Hierarchy
            layoutAnchor = tGO.transform;                        //Get its Transform
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
        var latestDrawnCard = piletop.Last();
        //print("The first drawn card is " + latestDrawnCard);
        latestDrawnCard.SetSortOrder(-1);
        latestDrawnCard.SetSortingLayerName("Target");


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

    public int discardcount = 2;
    void MoveToDiscard(CardPyramid cd)   //Moves the current target to the discardPile
    {
        cardClearCount += 1;
        piletop.Remove(cd);
        cd.state = eNewCardState.discard;  //Set the state of the card to discard
        discardPile.Add(cd);            //Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor;     //Update its transform parent

        //Position this card on the discardPile
        cd.faceUp = false; //so even if I set faceUp to false, that doesn't necessarily make the back active?
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.SetSortingLayerName(layout.discardPile.layerName);
        discardcount += 1;
        cd.SetSortOrder(-1 * discardcount);  //IT'S WORKING.
        if (piletop.Count > 0)
        {
            var setTarget = piletop.Last();
            setTarget.SetSortingLayerName("Target");
            setTarget.SetSortOrder(100);
        }
    }

    public float zlocation = 1.1F;
    void MoveToPiletop(CardPyramid cd)  //Function gets a card from draw() (removes a card from drawpile and returns it), places that card on the discardpile's top
        //for pyramid I need this to set the sorting order of each card moved to the piletop so it's a higher number than the discarded cards and in order of what was last drawn
    {
        //if (piletop != null) MoveToDiscard(piletop[-1]);
        //if (piletop == null) Draw();
        //var pileListLast = piletop.Last();
        //pileListLast.SetSortOrder(carddrawcount);
        //pileListLast = cd;
        zlocation += .1F;
        cd.state = eNewCardState.piletop;
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);
        //cd.transform.localPosition = new Vector3(
        //   layout.multiplier.x * layout.discardPile.x,
        //   layout.multiplier.y * layout.discardPile.y,
        //   zlocation);
        cd.faceUp = true; //Make it face-up
                          //Set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);  //changed "discardPile to drawPile since that should have a higher priority
        piletop.Add(cd);
        //        print("from MoveToPiletop, the card's layoutID is " + cd.layoutID);
        //cd.SetSortingLayerName("Target");
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

            cd.faceUp = false; //Changed this to false.
            //Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    //What's the easiest way to check if a card is a king, and then move it to the discard pile in each state where the card is clickable?
    //Okay, what cards are clickable and what aren't? Clickable: drawpile, piletop, row 0 and row 1

    public void CardClicked(CardPyramid cd)
    {
        //Trying to get card to change color when clicked
        //can i make it so everytime the piletop is clicked, it's only the target card that can be clicked?

        print("You have " + drawPile.Count + " cards left.");
        if (drawPile.Count == 0) 
        {
            CheckForGameOver();
        }


        switch (cd.state)
        {
            default:
                break;
            case eNewCardState.discard: //trying to set it so if a discard card is somehow clicked on the shared discard and target pile, then it's actually the piletop that is selected
                cd = null;
                print("You should not be able to select this card. (" + cd + ")");
                if (piletop == null)
                {
                    break;
                }

                //foreach (var item in piletop){
                //    print("This is an item in piletop: " + item);
                //}

                var discardToTarget = piletop.Last();
                print("The last item of piletop is " + discardToTarget);
                if (discardToTarget.rank == 13)
                {
                    var actualTop = piletop.Last();
                    actualTop.SetSortingLayerName("BelowTarget");
                    //belowTarget = actualTop;
                    actualTop.SetSortOrder(carddrawcount);
                    MoveToDiscard(actualTop);
                    firstCard = null;
                    secondCard = null;
                    actualTop = piletop.Last();
                    actualTop.SetSortingLayerName("Target");
                    actualTop.SetSortOrder(100); //not sure about this one yet.
                }
                if (firstCard == true)
                {
                    if (discardToTarget == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = discardToTarget;
                    //print("(cdisc) This card, " + discardToTarget + " has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        secondCard = null;
                        //MoveToPiletop(Draw());
                        var actualTop = piletop.Last();
                        actualTop.SetSortingLayerName("BelowTarget");
                        //belowTarget = actualTop;
                        UpdateDrawPile();
                        actualTop = piletop.Last();
                        actualTop.SetSortingLayerName("Target");
                    }
                    else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                }
                else
                {
                    firstCard = discardToTarget;
                    //print("(cdisc) This card, " + discardToTarget + " has been selected as card one.");
                }

                break;

            case eNewCardState.piletop:
                //print("This card is on the piletop");
                var aNewLastDrawn = piletop.Last(); 
                cd = aNewLastDrawn; //I wish I thought of this sooner.
                if (cd.rank == 13){
                    //var actualTop = piletop.Last();
                    //actualTop.SetSortingLayerName("BelowTarget");
                    //actualTop.SetSortOrder(carddrawcount);
                    //MoveToDiscard(actualTop);
                    MoveToDiscard(cd);
                    firstCard = null;
                    secondCard = null;
                    if (piletop.Count > 0)
                    {
                        var setTarget = piletop.Last();
                        setTarget.SetSortingLayerName("Target");
                        setTarget.SetSortOrder(100);
                    }
                    //actualTop = piletop.Last();
                    //actualTop.SetSortingLayerName("Target");
                    //actualTop.SetSortOrder(0); //not sure about this one yet.
                }
                if (firstCard == true){
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    //print("(cpt)This card, " + cd + " has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        if (tableau.Contains(firstCard) == true) {
                            tableau.Remove(firstCard);
                        }
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        if (tableau.Contains(secondCard) == true)
                        {
                            tableau.Remove(secondCard);
                        }
                        secondCard = null;
                        //MoveToPiletop(Draw());
                        if (piletop.Count > 1)
                        {

                            var actualTop = piletop.Last();
                            actualTop.SetSortingLayerName("BelowTarget");
                            //belowTarget = actualTop;
                            UpdateDrawPile();
                            actualTop = piletop.Last();
                            actualTop.SetSortingLayerName("Target");
                        }
                        else
                        {
                            UpdateDrawPile();
                        }

                    } else
                    {
                        firstCard = null;
                        secondCard = null;
                    }
                } else
                {
                    firstCard = cd;
                    //print("(cpt)This card, " + cd + " has been selected as card one.");
                    //cd.SetSortOrder(22); I think this was a test to see if this function actually worked.
                    //print("This card's sorting order should be 22.");
                }
                
                //code it to allow being selected and to glow
                //cd.

                break;

            case eNewCardState.drawpile:
                //Clicking any card in the drawPile will draw the next card
                //MoveToDiscard(piletop);
                //if (piletop[0] != null)
                //{
                //    var drawActualTop = piletop.Last();
                //    drawActualTop.SetSortOrder(carddrawcount);
                //}
                firstCard = null;
                carddrawcount += 3;
                if (piletop.Count > 0)
                {
                    var latestDrawnCard = piletop.Last();
                    latestDrawnCard.SetSortingLayerName("BelowTarget");
                    //belowTarget = latestDrawnCard;
                    latestDrawnCard.SetSortOrder(carddrawcount);
                    //latestDrawnCard.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Middle");
                    MoveToPiletop(Draw());
                    latestDrawnCard = piletop.Last();
                    //latestDrawnCard.GetComponent<Renderer>().sortingLayerID = SortingLayer.NameToID("Draw");
                    latestDrawnCard.SetSortingLayerName("Target");
                    //print("The last drawn card is " + latestDrawnCard);
                    latestDrawnCard.SetSortOrder(1);
                }
                else
                {
                    MoveToPiletop(Draw());
                }
                //print("The newest top should have the sorting layer of " + carddrawcount + ".");
                //for (int i = 0; i < piletop.Count; i++)
                //{
                //    foreach (var card in piletop)
                //    {
                //        card.SetSortOrder(100-i);
                //        //piletop.SetSortOrder(piletop.rank * -1); //change inside of SetSortOrder
                //    }
                //}
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
                    tableau.Remove(cd);
                    firstCard = null;
                    return;
                    //if (piletop == null)
                    //{
                    //    MoveToPiletop(Draw());
                    //}
                }


                if (firstCard == true)
                {
                    if (cd == firstCard)
                    {
                        firstCard = null;
                        return;
                    }
                    secondCard = cd;
                    //print("(ctab)This card, " + cd + " has been selected as card two.");
                    if (EqualsThirteen(firstCard, secondCard) == true)
                    {
                        MoveToDiscard(firstCard);
                        tableau.Remove(firstCard);
                        //for (int i = 0; i < discardPile.Count; i++)
                        //{
                        //    foreach (var card in discardPile)
                        //    {
                        //        card.SetSortOrder(0 - i);
                        //        //piletop.SetSortOrder(piletop.rank * -1); //change inside of SetSortOrder
                        //    }
                        //}
                        //firstCard.SetSortOrder(-10 * firstCard.rank); //maybe? nah
                        firstCard = null;
                        MoveToDiscard(secondCard);
                        tableau.Remove(secondCard);
                        //for (int i = 0; i < discardPile.Count; i++)
                        //{
                        //    foreach (var card in discardPile)
                        //    {
                        //        card.SetSortOrder(0 - i);
                        //        //piletop.SetSortOrder(piletop.rank * -1); //change inside of SetSortOrder
                        //    }
                        //}
                        //secondCard.SetSortOrder(-10 * secondCard.rank); //maybe? nah
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
                    //print("(ctab)This card, " + cd + " has been selected as card one.");
                }
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

    //public bool CheckForNoMoves()   //check if the target card and any of the uncovered cards in the tableau equal thirteen
    //{
    //    if (piletop.Count > 0 && tableau.Count > 0) { //if there are piletop cards at all and there are cards in the tableau
    //    var checkLastForMatch = piletop.Last();
    //        foreach (var cdd in tableau)
    //        {
    //            bool cardCovered = false;
    //            foreach (CardPyramid cover in cdd.hiddenBy)
    //            {
    //                if (cover.state == eNewCardState.tableau)   //if the card is being covered, continue to check the next card
    //                {
    //                    cardCovered = true;
    //                }
    //            }
    //            if (cardCovered == false && EqualsThirteen(cdd, checkLastForMatch) == true) //if it does, then return false, the game is still playable
    //            {
    //                return false;
    //            }
    //            else
    //            {
    //                return true;
    //            }
    //        }
    //        return false; //if the piletop is less than 0 and the tableau is less than 0 then the game must be over.
    //    }
    //    return false;
    //}

    public void CheckForGameOver()
    {
        print("CheckForGameOver() is running.");
        if (tableau.Count == 0)
        {
            ReloadLevel();
            return;
        }
        //if (CheckForNoMoves() == true)
        //{
        //    ReloadLevel();
        //    return;
        //}
        print("The game is not over yet.");
        return;
    }

    void ReloadLevel()
    {
        //Reload the scene, resetting the game
        SceneManager.LoadScene("GameScene");
    }
}
