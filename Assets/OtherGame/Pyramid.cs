using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pyramid : MonoBehaviour
{
    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public Transform layoutAnchor;
    public List<CardPyramid> drawPile;
    public List<CardPyramid> discardPile;

    // Start is called before the first frame update
    void Start()
    {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        layout = GetComponent<Layout>(); //get Layout component
        layout.ReadLayout(layoutXML.text); //Pass LayoutXML
        //drawPile = ConvertListCardsToListCardProspectors(deck.cards);

        //LayoutGame();
    }


    //void LayoutGame();
}
