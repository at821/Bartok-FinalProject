﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

    [Header("Set in Inspector")]

    public bool startFaceUp = false;

	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
	

	public GameObject prefabSprite;
	public GameObject prefabCard;

	[Header("Set Dynamically")]

	public PT_XMLReader					xmlr;
	
	public List<string>					cardNames;
	public List<Card>					cards;
	public List<Decorator>				decorators;
	public List<CardDefinition>			cardDefs;
	public Transform					deckAnchor;
	public Dictionary<string, Sprite>	dictSuits;
    

	
	public void InitDeck(string deckXMLText) {

		if( GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}
		
		// init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};
		
	
		ReadDeck (deckXMLText);
		MakeCards();
	}


	
	public void ReadDeck(string deckXMLText)
	{
		xmlr = new PT_XMLReader ();
		xmlr.Parse (deckXMLText);

		
		string s = "xml[0] decorator [0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		print (s);

        
		decorators = new List<Decorator>();
		
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i=0; i<xDecos.Count; i++) {
		
			deco = new Decorator();
			deco.type = xDecos[i].att ("type");
			deco.flip = (xDecos[i].att ("flip") == "1");   
			deco.scale = float.Parse (xDecos[i].att("scale"));
			deco.loc.x = float.Parse (xDecos[i].att("x"));
			deco.loc.y = float.Parse (xDecos[i].att("y"));
			deco.loc.z = float.Parse (xDecos[i].att("z"));
			decorators.Add (deco);
       
		}
		

		cardDefs = new List<CardDefinition>();
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		
		for (int i=0; i<xCardDefs.Count; i++) {
		
			CardDefinition cDef = new CardDefinition();
			cDef.rank = int.Parse(xCardDefs[i].att("rank"));
			
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {			
				for (int j = 0; j < xPips.Count; j++) {

                    deco = new Decorator();
					deco.type = "pip";
					deco.flip = (xPips[j].att ("flip") == "1");   
					deco.loc.x = float.Parse (xPips[j].att("x"));
					deco.loc.y = float.Parse (xPips[j].att("y"));
					deco.loc.z = float.Parse (xPips[j].att("z"));

                    if (xPips[j].HasAtt("scale") ) {
						deco.scale = float.Parse (xPips[j].att("scale"));
					}
					cDef.pips.Add (deco);
				} // for j
			}// if xPips
			
			
			if (xCardDefs[i].HasAtt("face")){
				cDef.face = xCardDefs[i].att ("face");
			}
			cardDefs.Add (cDef);
		} // for i < xCardDefs.Count

   
    } // ReadDeck

    public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank


    public void MakeCards() {

        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters) {
            for (int i = 0; i < 13; i++) {
                cardNames.Add(s + (i + 1));
            }
        }

        cards = new List<Card>();

        for (int i = 0; i < cardNames.Count; i++)        {

            cards.Add(MakeCard(i));
        }
    }

    private Card MakeCard(int cNum) {
        GameObject cgo = Instantiate(prefabCard) as GameObject;
        cgo.transform.parent = deckAnchor;
        Card card = cgo.GetComponent<Card>();

        cgo.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        card.name = cardNames[cNum];
        card.suit = card.name[0].ToString();
        card.rank = int.Parse(card.name.Substring(1));

        if (card.suit == "D" || card.suit == "H") {
            card.colS = "Red";
            card.color = Color.red;
        }

        card.def = GetCardDefinitionByRank(card.rank);

        AddDecorators(card);
        AddPips(card);
        AddFace(card);
        AddBack(card);

        return card;
    }

    private Sprite _tSp = null;
    private GameObject _tGO = null;
    private SpriteRenderer _tSR = null;

    private void AddDecorators(Card card) {
        foreach (Decorator deco in decorators) {
            if (deco.type == "suit") {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSR.sprite = dictSuits[card.suit];

            }
            else {
                _tGO = Instantiate(prefabSprite) as GameObject;
                _tSR = _tGO.GetComponent<SpriteRenderer>();
                _tSp = rankSprites[card.rank];
                _tSR.sprite = _tSp;
                _tSR.color = card.color;
            }

            _tSR.sortingOrder = 1;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = deco.loc;

            if (deco.flip) {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (deco.scale != 1) {
                _tGO.transform.localScale = Vector3.one * deco.scale;
            }

            _tGO.name = deco.type;

            card.decoGOs.Add(_tGO);
        } // foreach Deco
    }

    private void AddPips(Card card) {
        foreach (Decorator pip in card.def.pips) {
            _tGO = Instantiate(prefabSprite) as GameObject;
            _tGO.transform.SetParent(card.transform);
            _tGO.transform.localPosition = pip.loc;

            if (pip.flip) {
                _tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
            }

            if (pip.scale != 1) {
                _tGO.transform.localScale = Vector3.one * pip.scale;
            }

            _tGO.name = "pip";
            _tSR = _tGO.GetComponent<SpriteRenderer>();
            _tSR.sprite = dictSuits[card.suit];
            _tSR.sortingOrder = 1;
            card.pipGOs.Add(_tGO);
        }
    }

    private void AddFace(Card card)    {
        if (card.def.face == "")        {
            return;
        }
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSp = GetFace(card.def.face + card.suit);
        _tSR.sprite = _tSp;
        _tSR.sortingOrder = 1;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;  
        _tGO.name = "face";
    }
    
			
	/*		
			cards.Add (card);
		} // for all the Cardnames	
	} // makeCards
	*/


    private Sprite GetFace(string faceS) {
		foreach (Sprite _tSp in faceSprites) {
			if (_tSp.name == faceS) {
				return (_tSp);
			}
		}//foreach	
		return (null);  
	 }// getFace 


    private void AddBack(Card card)    {
        _tGO = Instantiate(prefabSprite) as GameObject;
        _tSR = _tGO.GetComponent<SpriteRenderer>();
        _tSR.sprite = cardBack;
        _tGO.transform.SetParent(card.transform);
        _tGO.transform.localPosition = Vector3.zero;
        _tSR.sortingOrder = 2;
        _tGO.name = "back";

        card.back = _tGO;
        card.faceUp = startFaceUp;
    }

	 static public void Shuffle(ref List<Card> oCards)	 {
	 	List<Card> tCards = new List<Card>();

	 	int ndx;   
        tCards = new List<Card>();
	 	while (oCards.Count > 0) 	 	{
	 		
	 		ndx = Random.Range(0,oCards.Count);
	 		tCards.Add(oCards[ndx]);
	 		oCards.RemoveAt(ndx);
	 	}

	 	oCards = tCards;

	 }


} // Deck class
