using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector : MonoBehaviour {

	static public Prospector S;

	[Header("Set in Inspector")]
	public TextAsset deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;


	[Header("Set Dynamically")]
	public Deck deck;
	public Layout layout;
	public List<CardProspector> drawPile;
	public Transform layoutAnchor;
	public CardProspector target;
	public List<CardProspector> tableau;
	public List<CardProspector> discardPile;

	void Awake() {
		S = this; //"set up singleton for Prospector"
	}

	void Start() {
		deck = GetComponent<Deck>();
		deck.InitDeck(deckXML.text);
		Deck.Shuffle(ref deck.cards);

		layout = GetComponent<Layout>(); //get Layout component
		layout.ReadLayout(layoutXML.text); //Pass LayoutXML
		drawPile = ConvertListCardsToListCardProspectors(deck.cards);

		LayoutGame();
	}

	List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) //Also a class
	{
		List<CardProspector> lCP = new List<CardProspector>();
		CardProspector tCP;
		foreach (Card tCD in lCD)
		{
			tCP = tCD as CardProspector;
			lCP.Add(tCP);
		}
		return (lCP);
	}

	CardProspector Draw() //what is this? It's a class :0
	{
		CardProspector cd = drawPile[0];
		drawPile.RemoveAt(0);
		return (cd);
	}

	void LayoutGame()       //positions initial tableau of cards ("mine")
	{
		if (layoutAnchor == null)           //Creates an empty GameObject to serve as an anchor for the tableau
		{
			GameObject tGO = new GameObject("_LayoutAnchor");       //Create empty GameObject named _LayoutAnchor in Hierarchy
			layoutAnchor = tGO.transform;           //Get its Transform
			layoutAnchor.transform.position = layoutCenter;     //Position it
		}
		CardProspector cp; //follow the layout
		
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
			cp.state = eCardState.tableau;
			//CardProspectors in the tableau have the state CardState.tableau
			cp.SetSortingLayerName(tSD.layerName); //Set the sorting layers

			tableau.Add(cp);    //Add this CardProspector to the List<> tableau
		}

		//Set which cards are hiding others
		foreach(CardProspector tCP in tableau)
        {
			foreach(int hid in tCP.slotDef.hiddenBy)
            {
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
            }
        }


        MoveToTarget(Draw()); //Set up the initial target card

        UpdateDrawPile();   //Set up the Draw pile
    }

	//Convert from the layoutID int to the CardProspector with that ID
	CardProspector FindCardByLayoutID(int layoutID)
    {
		foreach (CardProspector tCP in tableau)
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
	
	//turns cards in the mine face-up or face-down
	void SetTableauFaces()
    {
		foreach(CardProspector cd in tableau)
        {
			bool faceUp = true; //Assume the card will be face-up
			foreach(CardProspector cover in cd.hiddenBy)
            {
				//if either of the covering cards are in the tableau
				if(cover.state == eCardState.tableau)
                {
					faceUp = false; //then this card is face-down
                }
            }
			cd.faceUp = faceUp; //set the value on the card
        }
    }

	void MoveToDiscard(CardProspector cd)   //Moves the current target to the discardPile
	{
		cd.state = eCardState.discard;  //Set the state of the card to discard
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

    //Make cd the new target card
    void MoveToTarget(CardProspector cd)    //Make cd the new target
    {
        if (target != null) MoveToDiscard(target);  //if there is currently a target card, move it to discardPile
        target = cd;        //cd is the new target
        cd.state = eCardState.target;
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
        CardProspector cd;
        //Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            //Position it correctly with the layout.drawPile stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);

            cd.faceUp = false;
            //Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }
    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                //clicking the target card does nothing
                break;

            case eCardState.drawpile:
                //Clicking any card in the drawPile will draw the next card
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                break;

            case eCardState.tableau:
                bool validMatch = true;     //click a card in the tableau will check if it's a valid play
                if (!cd.faceUp) //if card is face-down, it's not a valid play
                {
                    validMatch = false;
                }
                if (!AdjacentRank(cd, target))  //if it's not an adjacent rank, it's not valid
                {
                    validMatch = false;
                }
                if (!validMatch) return;    //return if not valid

                tableau.Remove(cd); //Remove it from the tableau List
                MoveToTarget(cd); //Make it the target card
				SetTableauFaces();
                break;
        }
		CheckForGameOver();
    }

	void CheckForGameOver()
    {
		if(tableau.Count == 0)
        {
			GameOver(true);
			return;
        }
        if (drawPile.Count > 0)
        {
			return;
        }
		foreach(CardProspector cd in tableau)
        {
			if(AdjacentRank(cd, target))
            {
				//If there's a valid play, the game's not over
				return;
            }
        }

		GameOver(false);
    }

	void GameOver(bool won)
    {
        if (won)
        {
			print("Game Over. You won! :)");
        } else
        {
			print("Game Over. You lost. :(");
        }
		//reload the scene, resetting the game
		SceneManager.LoadScene("__Prospector_Scene_0");
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1)  //return true if the two cards are adjacent in rank (A & K adjacent)
    {
        //if either card is CubemapFace-Dropdown, it's not adjacent
        if (!c0.faceUp || !c1.faceUp) return (false);

        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }

        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        //otherwise return false; page 682, end 704
        return (false);
    }
}