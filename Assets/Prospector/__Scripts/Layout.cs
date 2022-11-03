using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] //Makes SlotDefs visible in Unity Inspector

public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}


public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr; //xml reader
    public PT_XMLHashtable xml; //for faster xml access
    public Vector2 multiplier; //"offset of tableau's center"

    //SlotDef references
    public List<SlotDef> slotDefs;  //All SlotDefs for Row0-Row3
    public SlotDef drawPile;        
    public SlotDef discardPile;     

    //Holds all of the possible names for layers by layerID
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };

    //Function reads LayoutXML.xml file
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);
        xml = xmlr.xml["xml"][0];

        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        SlotDef tSD;                            //Read in the slots

        PT_XMLHashList slotsX = xml["slot"];    //shortcut to the <slot>s

        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();                //Create a new SlotDef instance
            if(slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");   //If <slot> has a type attribute, parse
            } else
            {
                tSD.type = "slot";          //If not, set type to "slot"; it's a card in the rows
            }
            //Various attribs parsed into numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            //Converts number of the layerID into a text layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];     

            switch (tSD.type)   //pull additional attribs based on type of this <slot>
            {
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
}
