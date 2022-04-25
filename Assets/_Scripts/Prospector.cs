using UnityEngine;
using System.Collections.Generic;
using System;

public class Prospector : MonoBehaviour
{
    static public Prospector P;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float     xOffset = 3;
    public float     yOffset = -2.5f;
    public Vector3   layoutCenter;


    [Header("Set Dynamically")]
    public Deck                 deck;
    public Layout               layout;
    public Transform            layoutAnchor;
    public CardProspector       target;
    public List<CardProspector> tableau;
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;

    private void Awake() {
        P = this;
    }

    private void Start() {
        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards);

        // Card c;
        // for (int cNum = 0; cNum < deck.cards.Count; cNum++)
        // {
        //     c = deck.cards[cNum];
        //     c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
        // }

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        drawPile = ConvertCardsToCardProspectors(deck.cards);
        LayoutGame();
    }


    private List<CardProspector> ConvertCardsToCardProspectors(List<Card> cards)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;

        foreach (Card card in cards)
        {
            tCP = card as CardProspector;
            lCP.Add(tCP);
        }

        return lCP;
    }
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);

        return cd;
    }

    private void LayoutGame()
    {
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        foreach (SlotDef tSD in layout.slotDefs)
        {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x,
                                                     layout.multiplier.y * tSD.y,
                                                     -tSD.layerID);

            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);

            tableau.Add(cp);
        }
    }
}
