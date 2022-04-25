using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    public Vector2   fsPosMid = new Vector2( 0.5f, 0.90f ); 
    public Vector2   fsPosRun = new Vector2( 0.5f, 0.75f ); 
    public Vector2   fsPosMid2 = new Vector2( 0.4f, 1.0f ); 
    public Vector2   fsPosEnd = new Vector2( 0.5f, 0.95f );
    public float     reloadDelay = 2f;
    public Text      gameOverText;
    public Text      roundResultText;
    public Text      highScoreText;


    [Header("Set Dynamically")]
    public Deck                 deck;
    public Layout               layout;
    public Transform            layoutAnchor;
    public CardProspector       target;
    public List<CardProspector> tableau;
    public List<CardProspector> drawPile;
    public List<CardProspector> discardPile;
    public FloatingScore        fsRun;


    private void Awake() 
    {
        P = this;
        SetUpUITexts();
    }

    private void Start() 
    {
        Scoreboard.S.score = ScoreManager.SCORE;

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

        foreach (CardProspector tCP in tableau)
        {
            foreach (int hiddenID in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hiddenID);
                tCP.hiddenBy.Add(cp);
            }
        }

        MoveToTarget(Draw());
        UpdateDrawPile();
    }

    private CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in tableau)
        {
            if (tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }

        return null;
    }

    private void MoveToDiscard(CardProspector cd)
    {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
                                                 layout.multiplier.y * layout.discardPile.y,
                                                 -layout.discardPile.layerID + 0.5f);

        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    private void MoveToTarget(CardProspector cd)
    {
        if (target != null)
        {
            MoveToDiscard(target);
        }

        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x,
                                                 layout.multiplier.y * layout.discardPile.y,
                                                 -layout.discardPile.layerID);

        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    private void UpdateDrawPile()
    {
        CardProspector cd;

        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;

            float xCoord = layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x);
            float yCoord = layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y);
            float zCoord = -layout.drawPile.layerID + 0.1f * i;
            cd.transform.localPosition = new Vector3(xCoord, yCoord, zCoord);

            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.discardPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public void CardClicked(CardProspector cd)
    {
        switch (cd.state)
        {
            case eCardState.target:
                break;
            
            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(eScoreEvent.draw);
                FloatingScoreHandler(eScoreEvent.draw);

                break;

            case eCardState.tableau:
                bool validMatch = true;
                
                if (!cd.faceUp)
                {
                    validMatch = false;
                }

                if (!AdjacentRank(cd, target))
                {
                    validMatch = false;
                }

                if (!validMatch) return;

                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
                ScoreManager.EVENT(eScoreEvent.mine);
                FloatingScoreHandler(eScoreEvent.mine);

                break;
        }

        CheckForGameOver();
    }

    private void CheckForGameOver()
    {   
        // Check if the tableau is empty
        if (tableau.Count == 0)
        {
            GameOver(true);
            return;
        }

        // Check if there are still cards to draw
        if (drawPile.Count > 0)
        {
            return;
        }

        // Check for valid plays
        foreach (CardProspector cd in tableau)
        {
            if (AdjacentRank(cd, target))
            {
                return;
            }
        }

        GameOver(false);
    }

    private void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;

        if (fsRun != null)
        {
            score += fsRun.score;
        }

        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round! \n Round Score: " + score;
            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameWin);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score! \n High Score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }

            ShowResultsUI(true);
            ScoreManager.EVENT(eScoreEvent.gameLoss);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }

        Invoke("ReloadLevel", reloadDelay);
    }

    private void ReloadLevel() 
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    private void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;

        switch (evt)
        {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                if (fsRun != null)
                {
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);

                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    fsRun.fontSizes = new List<float>(new float[] {28,36,4});
                    fsRun = null;
                }
                break;

            case eScoreEvent.mine:
                FloatingScore fs;

                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;

                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);

                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });

                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }

                break;
        }
    }

    private void SetTableauFaces()
    {
        foreach (CardProspector cd in tableau)
        {
            bool faceUp = true;

            // If any of the covering cards are still in the tableau, 
            //  the card is face down.
            foreach (CardProspector cover in cd.hiddenBy)
            {
                if (cover.state == eCardState.tableau)
                {
                    faceUp = false;
                }
            }

            cd.faceUp = faceUp;
        }
    }

    private void SetUpUITexts()
    {
        // Set up HighScore Text
        GameObject go = GameObject.Find("HighScore");

        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }

        int highScore = ScoreManager.HIGH_SCORE;
        string highScoreString = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = highScoreString;

        // Set up Game Over Text
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        // Set up Round Result Text
        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }

        ShowResultsUI(false);
    }

    private void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    private bool AdjacentRank(CardProspector cd, CardProspector target)
    {   
        // If either card is face-down, it's not adjacent
        if (!cd.faceUp || !target.faceUp) return false;
        
        // If card rank is 1 rank apart
        if (Mathf.Abs(cd.rank - target.rank) == 1) return true;

        // If one card is an Ace and the other a King
        if (cd.rank == 1 && target.rank == 13) return true;
        if (cd.rank == 13 && target.rank == 1) return true;

        // Default case
        return false;
    }
}
