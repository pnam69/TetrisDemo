using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform boardParent;

    private Card firstCard;
    private Card secondCard;
    private int totalPairs;
    private int matchedPairs = 0;
    private bool canSelect = true;
    private int combo = 0;
    private float hintTimer = 0f;
    private float currentHintCooldown = 0f;
    [SerializeField] private GridLayoutGroup grid;
    private void Update()
    {
        if (currentHintCooldown > 0f)
            currentHintCooldown -= Time.deltaTime;
    }
    public void GenerateBoard()
    {
        if (boardParent == null)
        {
            Debug.LogWarning("BoardManager: boardParent is not assigned.");
            return;
        }

        // Clear existing cards
        for (int i = boardParent.childCount - 1; i >= 0; i--)
        {
            Transform child = boardParent.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        matchedPairs = 0;
        firstCard = null;
        secondCard = null;
        canSelect = true;

        if (cardPrefab == null)
        {
            Debug.LogWarning("BoardManager: cardPrefab is not assigned.");
            return;
        }
        totalPairs = GameManager.Instance.levelManager.GetCurrentLevel().pairCount;
        int pairCount = totalPairs;
        ConfigureGrid(pairCount);
        List<int> cardIDs = new List<int>();

        for (int i = 0; i < pairCount; i++)
        {
            cardIDs.Add(i);
            cardIDs.Add(i);
        }

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int randomIndex = Random.Range(i, cardIDs.Count);
            int temp = cardIDs[i];
            cardIDs[i] = cardIDs[randomIndex];
            cardIDs[randomIndex] = temp;
        }

        GridLayoutGroup grid = boardParent.GetComponent<GridLayoutGroup>();
        if (grid != null) grid.enabled = true;

        foreach (int id in cardIDs)
        {
            GameObject cardObj = Instantiate(cardPrefab, boardParent);
            Card card = cardObj.GetComponent<Card>();

            if (card != null)
            {
                card.SetCard(id);
                card.SetSize(GetCardSize());
            }
        }
        if (grid != null)
            StartCoroutine(DisableGridNextFrame(grid));
       // boardParent.GetComponent<GridLayoutGroup>().enabled = false;
    }
    private IEnumerator DisableGridNextFrame(GridLayoutGroup grid)
    {
        yield return null;
        grid.enabled = false;
    }

    public void ResetBoard()
    {
        matchedPairs = 0;
        firstCard = null;
        secondCard = null;
        canSelect = true;
    }

    public void CardSelected(Card card)
    {
        // track move
        if (GameManager.Instance != null)
        {
            GameManager.Instance.moveCount++;
            GameManager.Instance.UpdateUI();
        }

        if (!canSelect) return;

        if (firstCard == null)
        {
            firstCard = card;
            // reveal first card visually
            if (card != null) card.Flip();
        }
        else if (secondCard == null && card != firstCard)
        {
            secondCard = card;
            if (card != null) card.Flip();
            StartCoroutine(CheckMatch());
        }
    }

    private IEnumerator CheckMatch()
    {
        canSelect = false;

        yield return new WaitForSeconds(0.5f);

        if (firstCard != null && secondCard != null && firstCard.cardID == secondCard.cardID)
        {
            // match
            firstCard.Match();
            secondCard.Match();

            matchedPairs++;

            // combo / scoring
            combo++;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(10 * combo);
            }

            if (matchedPairs >= totalPairs)
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.WinGame();
            }
        }
        else
        {
            // no match - reset combo
            combo = 0;
            if (firstCard != null) firstCard.Hide();
            if (secondCard != null) secondCard.Hide();
        }

        firstCard = null;
        secondCard = null;
        canSelect = true;
    }

    public bool CanSelect()
    {
        return canSelect;
    }
    public void RevealOnePairTemporarily()
    {
        StartCoroutine(RevealPairRoutine());
    }

    private IEnumerator RevealPairRoutine()
    {
        List<Card> cards = new List<Card>();
        foreach (Transform t in boardParent)
        {
            Card c = t.GetComponent<Card>();
            if (c != null && !c.IsMatched)
                cards.Add(c);
        }

        // find any matching pair by id
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = i + 1; j < cards.Count; j++)
            {
                if (cards[i].cardID == cards[j].cardID)
                {
                    cards[i].RevealTemporary(1f);
                    cards[j].RevealTemporary(1f);
                    yield break;
                }
            }
        }
    }
    private void ConfigureGrid(int pairCount)
    {
        if (grid == null) return;

        grid.enabled = true;

        if (pairCount <= 2)
        {
            grid.constraintCount = 2;
            grid.cellSize = new Vector2(250, 350);
        }
        else if (pairCount <= 4)
        {
            grid.constraintCount = 4;
            grid.cellSize = new Vector2(250, 350);
        }
        else
        {
            grid.constraintCount = 6;
            grid.cellSize = new Vector2(250, 350);
        }
    }
    private Vector2 GetCardSize()
    {
        RectTransform boardRect = boardParent.GetComponent<RectTransform>();

        float width = boardRect.rect.width;
        float height = boardRect.rect.height;

        int pairCount = totalPairs;
        int cardCount = pairCount * 2;

        int columns = Mathf.CeilToInt(Mathf.Sqrt(cardCount));
        int rows = Mathf.CeilToInt((float)cardCount / columns);

        float cellW = width / columns;
        float cellH = height / rows;

        return new Vector2(cellW, cellH);
    }
    public void TryUseHint()
    {
        if (currentHintCooldown > 0f) return;

        RevealOnePairTemporarily();

        LevelData lvl = GameManager.Instance.levelManager.GetCurrentLevel();
        if (lvl != null)
            currentHintCooldown = lvl.hintCooldown;
    }
}
