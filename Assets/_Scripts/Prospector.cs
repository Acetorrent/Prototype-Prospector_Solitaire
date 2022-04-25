using UnityEngine;
using System.Collections.Generic;
using System;

public class Prospector : MonoBehaviour
{
    static public Prospector P;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;

    [Header("Set Dynamically")]
    public Deck                 deck;
    public Layout               layout;
    public List<CardProspector> drawPile;

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
}
