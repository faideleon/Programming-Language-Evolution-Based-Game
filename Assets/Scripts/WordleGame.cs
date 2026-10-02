using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Self-contained Wordle. Builds its own UI at runtime so the Wordle scene can be
// loaded on its own or additively on top of the main scene:
//   SceneManager.LoadScene("Wordle", LoadSceneMode.Additive);
// Listen to WordleGame.onWordleFinished to know if the player won.
public class WordleGame : MonoBehaviour
{
    [Serializable]
    public class WordEntry
    {
        public string word;
        public string category;
        public string hint;

        public WordEntry(string word, string category, string hint)
        {
            this.word = word;
            this.category = category;
            this.hint = hint;
        }
    }

    public static event Action<bool> onWordleFinished; // true = solved

    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private int maxGuesses = 6;
    [SerializeField] private bool showHint = true;
    [SerializeField] private bool closeSceneOnFinish = false;

    [SerializeField] private List<WordEntry> words = new List<WordEntry>
    {
        // Ancient programming languages / early computing
        new WordEntry("ABACUS", "Ancient Computing", "The oldest counting tool, made of beads"),
        new WordEntry("COBOL",  "Ancient Language",  "Old business language from 1959"),
        new WordEntry("BASIC",  "Ancient Language",  "Beginner's All-purpose Symbolic Instruction Code"),
        new WordEntry("LISP",   "Ancient Language",  "Old AI language full of (parentheses)"),
        new WordEntry("PASCAL", "Ancient Language",  "Named after a French mathematician"),
        new WordEntry("ALGOL",  "Ancient Language",  "ALGOrithmic Language from 1958"),
        new WordEntry("LOGO",   "Ancient Language",  "The language with the drawing turtle"),
        new WordEntry("ADA",    "Ancient Language",  "Named after the first programmer, Lovelace"),
        new WordEntry("ENIAC",  "Ancient Computing", "One of the first giant electronic computers"),
        new WordEntry("PUNCH",  "Ancient Computing", "Old programs were ___ cards"),
        new WordEntry("PROLOG", "Ancient Language",  "Old logic programming language"),
        new WordEntry("BINARY", "Ancient Computing", "Only 0s and 1s"),

        // Assembly language terms
        new WordEntry("MOV",    "Assembly", "Copies a value into a register"),
        new WordEntry("ADD",    "Assembly", "Sums two values"),
        new WordEntry("JMP",    "Assembly", "Jumps to another address"),
        new WordEntry("PUSH",   "Assembly", "Puts a value on top of the stack"),
        new WordEntry("POP",    "Assembly", "Takes a value off the stack"),
        new WordEntry("CALL",   "Assembly", "Runs a subroutine"),
        new WordEntry("NOP",    "Assembly", "No OPeration - does nothing"),
        new WordEntry("STACK",  "Assembly", "Last in, first out memory"),
        new WordEntry("BYTE",   "Assembly", "Eight bits"),
        new WordEntry("LABEL",  "Assembly", "A name marking a spot in code"),
        new WordEntry("HALT",   "Assembly", "Stops the processor"),
        new WordEntry("OPCODE", "Assembly", "The number that tells the CPU what to do"),
        new WordEntry("MEMORY", "Assembly", "Where data is stored, like RAM"),
        new WordEntry("LOAD",   "Assembly", "Read a value from memory"),
    };

    private static readonly Color BgColor       = new Color32(10, 10, 12, 252);
    private static readonly Color EmptyColor    = new Color32(58, 58, 60, 255);
    private static readonly Color TypedColor    = new Color32(86, 87, 88, 255);
    private static readonly Color CorrectColor  = new Color32(83, 141, 78, 255);
    private static readonly Color PresentColor  = new Color32(181, 159, 59, 255);
    private static readonly Color AbsentColor   = new Color32(40, 40, 42, 255);
    private static readonly Color KeyColor      = new Color32(129, 131, 132, 255);

    private enum LetterState { None = 0, Absent = 1, Present = 2, Correct = 3 }

    private const int MaxWordLength = 6;
    private const string KeyRows = "QWERTYUIOP|ASDFGHJKL|ZXCVBNM";

    private WordEntry current;
    private string answer;
    private int row;
    private string typed = "";
    private bool gameOver;
    private bool revealing;

    private Canvas canvas;
    private RectTransform gridRoot;
    private Image[,] tiles;
    private TMP_Text[,] tileTexts;
    private TMP_Text categoryText;
    private TMP_Text hintText;
    private TMP_Text messageText;
    private GameObject endButtons;
    private readonly Dictionary<char, Image> keyImages = new Dictionary<char, Image>();
    private readonly Dictionary<char, LetterState> keyStates = new Dictionary<char, LetterState>();

    void Awake()
    {
        if (font == null) font = TMP_Settings.defaultFontAsset;
        // Keep only valid words (letters only, 6 or fewer)
        words.RemoveAll(w => w == null || string.IsNullOrEmpty(w.word) || w.word.Length > MaxWordLength);

        EnsureCameraAndEventSystem();
        BuildUI();
    }

    void OnEnable()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        if (Keyboard.current != null) Keyboard.current.onTextInput -= OnTextInput;
    }

    void Start()
    {
        NewGame();
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) PressEnter();
        if (kb.backspaceKey.wasPressedThisFrame) PressBackspace();
    }

    // ---------------- Game logic ----------------

    public void NewGame()
    {
        StopAllCoroutines();
        current = words[UnityEngine.Random.Range(0, words.Count)];
        answer = current.word.ToUpperInvariant();
        row = 0;
        typed = "";
        gameOver = false;
        revealing = false;
        keyStates.Clear();
        foreach (var k in keyImages) k.Value.color = KeyColor;

        BuildGrid();
        categoryText.text = $"{current.category}  -  {answer.Length} letters";
        hintText.text = showHint ? "Hint: " + current.hint : "";
        messageText.text = "";
        endButtons.SetActive(false);
    }

    private void OnTextInput(char c)
    {
        c = char.ToUpperInvariant(c);
        if (c >= 'A' && c <= 'Z') PressLetter(c);
    }

    public void PressLetter(char c)
    {
        if (gameOver || revealing || typed.Length >= answer.Length) return;
        typed += c;
        RefreshRow();
    }

    public void PressBackspace()
    {
        if (gameOver || revealing || typed.Length == 0) return;
        typed = typed.Substring(0, typed.Length - 1);
        RefreshRow();
    }

    public void PressEnter()
    {
        if (gameOver || revealing) return;
        if (typed.Length < answer.Length)
        {
            ShowMessage("Not enough letters");
            StartCoroutine(Shake(row));
            return;
        }
        StartCoroutine(RevealRow(row, typed, Evaluate(typed, answer)));
    }

    // Standard Wordle scoring, handles repeated letters correctly
    private static LetterState[] Evaluate(string guess, string target)
    {
        var result = new LetterState[guess.Length];
        var remaining = new Dictionary<char, int>();

        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] == target[i]) result[i] = LetterState.Correct;
            else
            {
                remaining.TryGetValue(target[i], out int n);
                remaining[target[i]] = n + 1;
            }
        }
        for (int i = 0; i < guess.Length; i++)
        {
            if (result[i] == LetterState.Correct) continue;
            if (remaining.TryGetValue(guess[i], out int n) && n > 0)
            {
                result[i] = LetterState.Present;
                remaining[guess[i]] = n - 1;
            }
            else result[i] = LetterState.Absent;
        }
        return result;
    }

    private IEnumerator RevealRow(int r, string guess, LetterState[] states)
    {
        revealing = true;
        for (int i = 0; i < guess.Length; i++)
        {
            yield return Flip(tiles[r, i].rectTransform, tiles[r, i], StateColor(states[i]));
            UpdateKey(guess[i], states[i]);
        }

        bool won = guess == answer;
        row++;
        typed = "";
        revealing = false;

        if (won)
        {
            string[] praise = { "Genius!", "Magnificent!", "Impressive!", "Splendid!", "Great!", "Phew!" };
            EndGame(true, praise[Mathf.Clamp(r, 0, praise.Length - 1)]);
        }
        else if (row >= maxGuesses)
        {
            EndGame(false, "The word was " + answer);
        }
    }

    private void EndGame(bool won, string msg)
    {
        gameOver = true;
        ShowMessage(msg, persistent: true);
        endButtons.SetActive(true);
        onWordleFinished?.Invoke(won);
        if (closeSceneOnFinish) StartCoroutine(CloseAfter(2f));
    }

    private IEnumerator CloseAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Close();
    }

    // Unloads this scene when it was loaded on top of another one
    public void Close()
    {
        if (SceneManager.sceneCount > 1) SceneManager.UnloadSceneAsync(gameObject.scene);
        else gameObject.SetActive(false);
    }

    private void UpdateKey(char c, LetterState state)
    {
        keyStates.TryGetValue(c, out var old);
        if (state <= old) return;
        keyStates[c] = state;
        if (keyImages.TryGetValue(c, out var img)) img.color = StateColor(state);
    }

    private static Color StateColor(LetterState s)
    {
        switch (s)
        {
            case LetterState.Correct: return CorrectColor;
            case LetterState.Present: return PresentColor;
            case LetterState.Absent: return AbsentColor;
            default: return EmptyColor;
        }
    }

    private void RefreshRow()
    {
        if (row >= maxGuesses) return;
        for (int i = 0; i < answer.Length; i++)
        {
            bool has = i < typed.Length;
            tileTexts[row, i].text = has ? typed[i].ToString() : "";
            tiles[row, i].color = has ? TypedColor : EmptyColor;
        }
    }

    // ---------------- Animations (unscaled, works while Time.timeScale = 0) ----------------

    private IEnumerator Flip(RectTransform t, Image img, Color target)
    {
        const float half = 0.12f;
        for (float e = 0; e < half; e += Time.unscaledDeltaTime)
        {
            t.localScale = new Vector3(1, 1 - e / half, 1);
            yield return null;
        }
        img.color = target;
        for (float e = 0; e < half; e += Time.unscaledDeltaTime)
        {
            t.localScale = new Vector3(1, e / half, 1);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private IEnumerator Shake(int r)
    {
        var rowRect = tiles[r, 0].rectTransform.parent as RectTransform;
        Vector2 start = rowRect.anchoredPosition;
        for (float e = 0; e < 0.3f; e += Time.unscaledDeltaTime)
        {
            rowRect.anchoredPosition = start + new Vector2(Mathf.Sin(e * 60f) * 8f, 0);
            yield return null;
        }
        rowRect.anchoredPosition = start;
    }

    private Coroutine messageRoutine;

    private void ShowMessage(string msg, bool persistent = false)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageText.text = msg;
        if (!persistent) messageRoutine = StartCoroutine(ClearMessage());
    }

    private IEnumerator ClearMessage()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (!gameOver) messageText.text = "";
    }

    // ---------------- UI construction ----------------

    private void EnsureCameraAndEventSystem()
    {
        // Only needed when the scene is played on its own
        if (Camera.main == null && FindAnyObjectByType<Camera>() == null)
        {
            var cam = new GameObject("Wordle Camera").AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.transform.SetParent(transform);
        }
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.transform.SetParent(transform);
        }
    }

    private void BuildUI()
    {
        var canvasGO = new GameObject("WordleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // draw above the main scene's UI
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        // Dim background that also blocks clicks to the scene below
        var bg = CreateImage("Background", canvasGO.transform, BgColor);
        Stretch(bg.rectTransform);

        var title = CreateText("Title", canvasGO.transform, "CODE WORDLE", 56, Color.white);
        Place(title.rectTransform, new Vector2(0, -25), new Vector2(1000, 70), top: true);

        categoryText = CreateText("Category", canvasGO.transform, "", 26, new Color(0.75f, 0.75f, 0.75f));
        Place(categoryText.rectTransform, new Vector2(0, -98), new Vector2(1200, 36), top: true);

        hintText = CreateText("Hint", canvasGO.transform, "", 22, new Color(0.9f, 0.8f, 0.45f));
        Place(hintText.rectTransform, new Vector2(0, -138), new Vector2(1400, 36), top: true);

        messageText = CreateText("Message", canvasGO.transform, "", 30, Color.white);
        Place(messageText.rectTransform, new Vector2(0, -178), new Vector2(1200, 45), top: true);

        gridRoot = new GameObject("Grid", typeof(RectTransform)).GetComponent<RectTransform>();
        gridRoot.SetParent(canvasGO.transform, false);
        Place(gridRoot, new Vector2(0, 40), new Vector2(600, 420), top: false);

        BuildKeyboard(canvasGO.transform);
        BuildEndButtons(canvasGO.transform);

        var close = CreateButton("CloseButton", canvasGO.transform, "X", 32, new Color32(150, 50, 50, 255), Close);
        var crt = close.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(1, 1);
        crt.anchoredPosition = new Vector2(-30, -30);
        crt.sizeDelta = new Vector2(70, 70);
    }

    private void BuildGrid()
    {
        foreach (Transform child in gridRoot) Destroy(child.gameObject);

        int len = answer.Length;
        const float size = 64f, gap = 6f;
        tiles = new Image[maxGuesses, len];
        tileTexts = new TMP_Text[maxGuesses, len];

        float totalW = len * size + (len - 1) * gap;
        float totalH = maxGuesses * size + (maxGuesses - 1) * gap;

        for (int r = 0; r < maxGuesses; r++)
        {
            var rowRT = new GameObject("Row" + r, typeof(RectTransform)).GetComponent<RectTransform>();
            rowRT.SetParent(gridRoot, false);
            rowRT.sizeDelta = new Vector2(totalW, size);
            rowRT.anchoredPosition = new Vector2(0, totalH / 2 - size / 2 - r * (size + gap));

            for (int c = 0; c < len; c++)
            {
                var tile = CreateImage("Tile" + c, rowRT, EmptyColor);
                tile.rectTransform.sizeDelta = new Vector2(size, size);
                tile.rectTransform.anchoredPosition = new Vector2(-totalW / 2 + size / 2 + c * (size + gap), 0);
                var txt = CreateText("Letter", tile.transform, "", 34, Color.white);
                Stretch(txt.rectTransform);
                tiles[r, c] = tile;
                tileTexts[r, c] = txt;
            }
        }
    }

    private void BuildKeyboard(Transform parent)
    {
        string[] rows = KeyRows.Split('|');
        const float kw = 64f, kh = 80f, gap = 8f;
        float baseY = 300f; // from the bottom

        for (int r = 0; r < rows.Length; r++)
        {
            string keys = rows[r];
            bool last = r == rows.Length - 1;
            float wideW = kw * 1.8f;
            float rowW = keys.Length * kw + (keys.Length - 1) * gap + (last ? 2 * (wideW + gap) : 0);
            float x = -rowW / 2;
            float y = baseY - r * (kh + gap);

            if (last)
            {
                AddKey(parent, "ENTER", 17, new Vector2(x + wideW / 2, y), new Vector2(wideW, kh), PressEnter);
                x += wideW + gap;
            }
            foreach (char k in keys)
            {
                char captured = k;
                var img = AddKey(parent, k.ToString(), 30, new Vector2(x + kw / 2, y), new Vector2(kw, kh), () => PressLetter(captured));
                keyImages[k] = img;
                x += kw + gap;
            }
            if (last)
                AddKey(parent, "DEL", 17, new Vector2(x + wideW / 2, y), new Vector2(wideW, kh), PressBackspace);
        }
    }

    private Image AddKey(Transform parent, string label, int fontSize, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        var btn = CreateButton("Key_" + label, parent, label, fontSize, KeyColor, onClick);
        var rt = btn.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return btn.GetComponent<Image>();
    }

    private void BuildEndButtons(Transform parent)
    {
        endButtons = new GameObject("EndButtons", typeof(RectTransform));
        endButtons.transform.SetParent(parent, false);
        var rt = endButtons.GetComponent<RectTransform>();
        Place(rt, new Vector2(0, -230), new Vector2(600, 55), top: true);

        var again = CreateButton("PlayAgain", endButtons.transform, "PLAY AGAIN", 20, CorrectColor, NewGame);
        var art = again.GetComponent<RectTransform>();
        art.sizeDelta = new Vector2(260, 55);
        art.anchoredPosition = new Vector2(-140, 0);

        var close = CreateButton("Continue", endButtons.transform, "CONTINUE", 20, KeyColor, Close);
        var crt = close.GetComponent<RectTransform>();
        crt.sizeDelta = new Vector2(260, 55);
        crt.anchoredPosition = new Vector2(140, 0);

        endButtons.SetActive(false);
    }

    private Image CreateImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    private TMP_Text CreateText(string name, Transform parent, string text, float size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        t.fontStyle = FontStyles.Bold;
        t.raycastTarget = false;
        return t;
    }

    private Button CreateButton(string name, Transform parent, string label, float fontSize, Color color, UnityEngine.Events.UnityAction onClick)
    {
        var img = CreateImage(name, parent, color);
        var btn = img.gameObject.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        var txt = CreateText("Label", img.transform, label, fontSize, Color.white);
        Stretch(txt.rectTransform);
        return btn;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void Place(RectTransform rt, Vector2 pos, Vector2 size, bool top)
    {
        var anchor = top ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0.5f);
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, top ? 1f : 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
