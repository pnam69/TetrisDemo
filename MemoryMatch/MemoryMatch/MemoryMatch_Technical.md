# T?i li?u k? thu?t ? Memory Match (Unity)

## 1) T?ng quan
- Th? lo?i: Puzzle / Memory Match.
- M?c ti?u: ng??i ch?i l?t th? ?? t?m c?p gi?ng nhau trong th?i gian gi?i h?n.
- N?n t?ng: Unity (scripts n?m trong `Assets/Scripts`).
- D? li?u level: `LevelData` (ScriptableObject) g?m `pairCount`, `timeLimit`, `baseScore`, `hintCooldown`.

## 2) Quy t?c ch?i
1. M?i l??t ng??i ch?i l?t 2 th?.
2. N?u tr?ng h?nh Å® gi? m?, c?ng ?i?m v? t?ng combo.
3. N?u kh?c Å® ?p l?i, combo reset.
4. Ho?n th?nh khi gh?p ?? t?t c? c?p.
5. H?t th?i gian (`timeRemaining <= 0`) Å® thua.

## 3) K?ch b?n ch?i (Gameplay loop)
1. `StartGame()` reset ?i?m, level, timer v? t?o board m?i.
2. `BoardManager.GenerateBoard()` sinh danh s?ch c?p, shuffle, g?n sprite cho m?i c?p.
3. Ng??i ch?i ch?n th? Å® `Card.Flip()` Å® `GameManager.CardSelected()`.
4. Khi ?? 2 th? Å® `BoardManager.CheckMatch()` ki?m tra ??ng/sai.
5. ??ng Å® gi? th? m? + t?ng ?i?m/combo. Sai Å® ?p l?i.
6. N?u `matchedPairs >= totalPairs` Å® `WinGame()`.
7. N?u `timeRemaining <= 0` Å® `GameOver()`.
8. `NextRound()` chuy?n level (n?u c?n) v? t?o board m?i.

## 4) Ki?n tr?c & th?nh ph?n ch?nh
- `GameManager`:
  - Qu?n l? tr?ng th?i game (start/pause/over/win).
  - Qu?n l? score, level, timer, UI panel.
  - ?i?u ph?i `BoardManager` v? `LevelManager`.
- `BoardManager`:
  - T?o board, shuffle card IDs, g?n sprite.
  - X? l? ch?n th?, match/mismatch, combo.
  - Hint system (l?t t?m m?t c?p).
- `Card`:
  - Hi?n th? sprite m?t tr??c/m?t sau.
  - Flip animation, tr?ng th?i `isFlipped` / `isMatched`.
- `LevelManager` + `LevelData`:
  - Danh s?ch level v? th?ng s? level hi?n t?i.

## 5) C?c b??c x?y d?ng game t? ??u (t?m t?t)
1. T?o Unity project 2D.
2. T?o scene ch?nh v? Canvas.
3. T?o prefab `Card`:
   - `Image` cho m?t tr??c (`frontImage`).
   - `CanvasGroup` cho m?t sau (`backImage`).
   - Script `Card` + m?ng `faceSprites`.
4. T?o `boardParent` v?i `GridLayoutGroup`.
5. Th?m `BoardManager` v?o scene, g?n `cardPrefab`, `boardParent`.
6. T?o `LevelData` assets v? g?n v?o `LevelManager`.
7. Th?m `GameManager` v? g?n UI: score/level/time/moves/hint, c?c panel.
8. G?n ?m thanh v?o `GameManager` (`flipSound`, `matchSound`).

## 6) M? quan tr?ng

### 6.1 Sinh board + shuffle ID (file: `Assets/Scripts/BoardManager.cs`)
```csharp
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
```

### 6.2 G?n sprite cho m?i c?p (random h?a) (file: `Assets/Scripts/BoardManager.cs`)
```csharp
int spriteCount = (proto != null && proto.faceSprites != null) ? proto.faceSprites.Length : 0;

List<int> pairSpriteList = new List<int>();
if (spriteCount <= 0)
{
    for (int i = 0; i < pairCount; i++) pairSpriteList.Add(0);
}
else
{
    List<int> tempPool = new List<int>(spritePool);
    Shuffle(tempPool);

    while (pairSpriteList.Count < pairCount)
    {
        if (tempPool.Count == 0)
        {
            tempPool = new List<int>(spritePool);
            Shuffle(tempPool);
        }

        pairSpriteList.Add(tempPool[0]);
        tempPool.RemoveAt(0);
    }
}
```

### 6.3 Flip th? + g?i ch?n th? (file: `Assets/Scripts/Card.cs`)
```csharp
public void Flip()
{
    if (isFlipped || isMatched) return;
    if (backImage == null) return;
    if (!GameManager.Instance.CanSelect()) return;

    isFlipped = true;
    StartCoroutine(FlipAnimation(true));
    backImage.blocksRaycasts = false;
    backImage.interactable = false;

    GameManager.Instance.PlayFlip();
    GameManager.Instance.CardSelected(this);
}
```

### 6.4 Ki?m tra match (file: `Assets/Scripts/BoardManager.cs`)
```csharp
if (firstCard != null && secondCard != null && firstCard.cardID == secondCard.cardID)
{
    firstCard.Match();
    secondCard.Match();
    matchedPairs++;
    combo++;
    if (GameManager.Instance != null)
        GameManager.Instance.AddScore(10 * combo);
}
else
{
    combo = 0;
    if (firstCard != null) firstCard.Hide();
    if (secondCard != null) secondCard.Hide();
}
```

### 6.5 Timer v? GameOver (file: `Assets/Scripts/GameManager.cs`)
```csharp
timeRemaining -= Time.deltaTime;
if (timeRemaining <= 0f)
{
    GameOver();
}
```

### 6.6 Hint system (file: `Assets/Scripts/BoardManager.cs`)
```csharp
public void TryUseHint()
{
    if (currentHintCooldown > 0f) return;

    RevealOnePairTemporarily();

    LevelData lvl = GameManager.Instance.levelManager.GetCurrentLevel();
    if (lvl != null)
        currentHintCooldown = lvl.hintCooldown;
}
```

### 6.7 Th?ng v?n & chuy?n round (file: `Assets/Scripts/GameManager.cs`)
```csharp
public void WinGame()
{
    isStarted = false;
    Time.timeScale = 0f;

    if (winPanel != null)
        winPanel.SetActive(true);

    if (winText != null)
    {
        winText.text = "Score: " + score + "\nMoves: " + moveCount + "\nTime Left: " + Mathf.Ceil(timeRemaining);
    }
}
```

```csharp
public void NextRound()
{
    if (winPanel != null)
        winPanel.SetActive(false);

    bool hasNextLevel = false;
    if (levelManager != null)
        hasNextLevel = levelManager.NextLevel();

    if (!hasNextLevel)
    {
        MainMenuAfterFinish();
        return;
    }

    LevelData lvl = levelManager.GetCurrentLevel();
    level = levelManager.currentLevel + 1;
    timeRemaining = lvl.timeLimit;
    moveCount = 0;

    if (boardManager != null)
    {
        boardManager.ResetBoard();
        boardManager.GenerateBoard();
    }

    UpdateUI();
    isStarted = true;
    Time.timeScale = 1f;
}
```

## 7) ?nh giao di?n Unity (?? ch?n)
1. Main Scene
2. Board layout + GridLayoutGroup
3. Card prefab (front/back)
4. LevelData assets
5. Game HUD + Win/GameOver panels

## 8) Tham chi?u nhanh theo ch?c n?ng
1. Sinh board & random h?nh: 6.1, 6.2
2. Flip v? ch?n th?: 6.3
3. Match/Mismatch: 6.4
4. Timer + GameOver: 6.5
5. Hint: 6.6
6. Win + NextRound: 6.7
