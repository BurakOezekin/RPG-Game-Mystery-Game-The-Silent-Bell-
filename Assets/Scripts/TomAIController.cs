using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TomAIController : MonoBehaviour, Interactable
{
    [Header("Waypoints (set in inspector or use defaults)")]
    public Vector3[] waypoints = new Vector3[] {
        new Vector3(-13f, 8f, 0f),
        new Vector3(-3f, 8f, 0f),
        new Vector3(-3f, 17f, 0f)
    };

    [Header("Movement")]
    public float speed = 2f;
    public float waitTime = 1.0f;
    public bool startOnAwake = true;

    [Header("Flee Behavior")]
    public float fleeDistance = 3f;
    public float fleeSpeed = 4f;
    public Transform playerTransform;
    private bool isFleeing = false;

    private int index = 0;
    private int direction = 1; // 1 = forward through array, -1 = backward
    private Coroutine routine;
    private Animator animator;
    private Rigidbody2D rb2d;

    // --- Dialog / Gemini fields (adapted from MaxAIController) ---
    [Header("Dialog (optional)")]
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] InputField inputField;
    private Dialog dialog;

    [Header("Gemini API Configuration (optional)")]
    [SerializeField] private string geminiApiKey = "";
    [SerializeField] private string geminiModel = "gemini-2.0-flash";
    [SerializeField] private string geminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    [SerializeField] private string geminiEnvVarName = "GEMINI_API_KEY";

    private bool isDialogOpen = false;
    private bool isWaitingForResponse = false;
    private int currentLine = 0;

    void Awake()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        
        // Try to find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }
    }

    void Start()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        transform.position = waypoints[0];

        if (startOnAwake)
            routine = StartCoroutine(FollowPath());
    }

    void Update()
    {
        // Check distance to player and flee if too close (unless dialog is open)
        if (!isDialogOpen && playerTransform != null && !isFleeing)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance < fleeDistance)
            {
                StartCoroutine(FleeFromPlayer());
            }
        }
        
        // Dialog controls (ENTER, L, ESC)
        HandleDialogInput();
    }

    IEnumerator FleeFromPlayer()
    {
        isFleeing = true;
        StopPatrol();

        float fleeDuration = 2f;
        float elapsed = 0f;

        while (elapsed < fleeDuration && playerTransform != null)
        {
            Vector2 fleeDirection = ((Vector2)transform.position - (Vector2)playerTransform.position).normalized;
            Vector2 targetPos = (Vector2)transform.position + fleeDirection * fleeSpeed * Time.deltaTime;

            if (rb2d != null)
                rb2d.MovePosition(targetPos);
            else
                transform.position = targetPos;

            UpdateAnimatorMovement(new Vector3(fleeDirection.x, fleeDirection.y, 0f));

            elapsed += Time.deltaTime;
            yield return null;
        }

        UpdateAnimatorMovement(Vector3.zero);
        isFleeing = false;
        
        // Resume patrol after fleeing
        yield return new WaitForSeconds(1f);
        StartPatrol();
    }

    IEnumerator FollowPath()
    {
        while (true)
        {
            Vector3 target = waypoints[index];

            // move towards target (use Rigidbody2D if present so physics blocks work)
            if (rb2d != null)
            {
                while (Vector2.Distance(rb2d.position, new Vector2(target.x, target.y)) > 0.05f)
                {
                    Vector2 next = Vector2.MoveTowards(rb2d.position, new Vector2(target.x, target.y), speed * Time.deltaTime);
                    Vector2 moveDir = (next - rb2d.position).normalized;
                    rb2d.MovePosition(next);
                    UpdateAnimatorMovement(new Vector3(moveDir.x, moveDir.y, 0f));
                    yield return null;
                }
            }
            else
            {
                while (Vector3.Distance(transform.position, target) > 0.05f)
                {
                    Vector3 dir = (target - transform.position).normalized;
                    Vector3 move = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                    Vector3 moveDir = (move - transform.position).normalized;
                    transform.position = move;
                    UpdateAnimatorMovement(moveDir);
                    yield return null;
                }
            }

            // reached waypoint
            UpdateAnimatorMovement(Vector3.zero);
            yield return new WaitForSeconds(waitTime);

            // advance index; reverse when reaching ends to create back-and-forth loop
            if (index == waypoints.Length - 1) direction = -1;
            else if (index == 0) direction = 1;
            index += direction;
        }
    }

    void UpdateAnimatorMovement(Vector3 moveDir)
    {
        if (animator == null) return;

        bool isMoving = moveDir.sqrMagnitude > 0.001f;
        animator.SetBool("isMoving", isMoving);

        float mx = 0f;
        float my = 0f;

        if (isMoving)
        {
            // emulate PlayerController's discrete direction (no diagonal)
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y))
            {
                mx = Mathf.Sign(moveDir.x);
                my = 0f;
            }
            else
            {
                mx = 0f;
                my = Mathf.Sign(moveDir.y);
            }
        }

        animator.SetFloat("moveX", mx);
        animator.SetFloat("moveY", my);
    }

    // Public controls
    public void StartPatrol()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(FollowPath());
    }

    public void StopPatrol()
    {
        if (routine != null) StopCoroutine(routine);
        routine = null;
        UpdateAnimatorMovement(Vector3.zero);
    }

    // --- Interactable implementation (opens dialog + Gemini) ---
    public void Interact()
    {
        if (!isDialogOpen)
        {
            // Mark Tom as talked to
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.MarkTomTalked();
            }
            
            // Stop fleeing and patrol
            isFleeing = false;
            StopPatrol();

            if (dialogBox != null)
            {
                dialogBox.SetActive(true);
                if (dialogText != null) dialogText.text = "Hallo, ich bin Tom! Stell mir ein Rätsel oder frag mich etwas:";
                if (inputField != null)
                {
                    inputField.text = "";
                    inputField.gameObject.SetActive(true);
                    inputField.ActivateInputField();
                }
            }

            isDialogOpen = true;
            PlayerController.canMove = false;
        }
    }

    void HandleDialogInput()
    {
        // ENTER → Anfrage senden
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Return) && !isWaitingForResponse)
        {
            string prompt = inputField != null ? inputField.text.Trim() : "";
            if (prompt.Length > 0)
            {
                if (inputField != null) inputField.text = "";
                StartCoroutine(SendToApi(prompt));
            }
        }

        // L → nächste Seite
        if (isDialogOpen && Input.GetKeyDown(KeyCode.L) && !isWaitingForResponse)
        {
            if (dialog != null && dialog.Lines != null && dialog.Lines.Count > 0)
            {
                currentLine++;
                if (currentLine < dialog.Lines.Count)
                {
                    if (dialogText != null) dialogText.text = dialog.Lines[currentLine];
                }
                else
                {
                    if (dialogText != null) dialogText.text = "\n(Drücke ESC zum Schließen)";
                }
            }
        }

        // ESC → schließen
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            if (dialogBox != null) dialogBox.SetActive(false);
            isDialogOpen = false;
            PlayerController.canMove = true;
            // resume patrol
            StartPatrol();
        }
    }

    IEnumerator SendToApi(string prompt)
    {
        isWaitingForResponse = true;
        if (inputField != null) inputField.gameObject.SetActive(false);
        if (dialogText != null) dialogText.text = "Tom denkt nach...";

        // Frame replies via a system prompt (Tom only knows the game world)
        string lower = prompt.ToLower();
        if (lower.Contains("glocke") || lower.Contains("bell") || lower.Contains("wo") || lower.Contains("hilfe") || lower.Contains("help"))
        {
            // Tom redirects to Max with a clear system prompt
            string systemPrompt = "SYSTEM: Du bist Tom, ein Dorfbewohner im mittelalterlichen RPG 'Die stille Glocke'. " +
                "Regeln: 1) Du kennst nur Dinge aus dieser Welt (Dorf, Burg, Glocke, Max). 2) Bei modernen Themen (Auto, Computer) antworte: 'Davon habe ich noch nie gehört!'. 3) Wenn der Spieler nach der Glocke fragt, leite ihn zu Max an. " +
                "Aufgabe: Antworte kurz (1-2 Sätze) auf Deutsch und drücke sinngemäß aus: 'Geh zur Burg und sprich mit Max dem Wächter. Er kennt ein Rätsel, das dich zur Glocke führt!'. Verwende SYNONYME oder eine PARAPHRASE, nicht exakt denselben Satz. " +
                "Eingabe des Spielers: ";
            
            prompt = systemPrompt + prompt;
        }
        else if (lower.Contains("rätsel") || lower.Contains("riddle") || lower.Contains("puzzle") || lower.Contains("antwort") || lower.Contains("answer"))
        {
            // Tom keeps redirecting to Max; still via system prompt
            string systemPrompt = "SYSTEM: Du bist Tom aus dem mittelalterlichen Dorf 'Die stille Glocke'. " +
                "Regeln: 1) Du kennst nur die Spielwelt. 2) Antworte knapp. 3) Wenn nach Rätsel/Antwort gefragt wird, verweise auf Max. " +
                "Aufgabe: Antworte (1 Satz) auf Deutsch: 'Das Rätsel kennt nur Max bei der Burg. Frage ihn!'. " +
                "Eingabe des Spielers: ";
            
            prompt = systemPrompt + prompt;
        }
        else
        {
            // General system prompt for Tom
            string systemPrompt = "SYSTEM: Du bist Tom, ein Dorfbewohner im mittelalterlichen RPG 'Die stille Glocke'. " +
                "Regeln: Du kennst nur die Spielwelt (Dorf, Burg, Glocke, Max, Peter). Bei modernen Dingen antworte: 'Davon habe ich noch nie gehört!'. Antworte immer kurz (1-2 Sätze) auf Deutsch und bleibe im Charakter. " +
                "Eingabe des Spielers: ";
            
            prompt = systemPrompt + prompt;
        }

        string body = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt.Replace("\"", "\\\"") + "\"}]}]}";

        var request = new UnityWebRequest(geminiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        string apiKey = null;
#if UNITY_EDITOR
        try
        {
            string secretsPath = Path.Combine(Application.dataPath, "..", "Secrets", "gemini_key.txt");
            if (File.Exists(secretsPath))
            {
                apiKey = File.ReadAllText(secretsPath).Trim();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    Debug.Log("[TomAIController] Loaded Gemini API key from Secrets/gemini_key.txt (Editor-only).");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[TomAIController] Failed reading Secrets/gemini_key.txt: " + ex.Message);
        }
#endif

        if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(geminiEnvVarName))
        {
            apiKey = System.Environment.GetEnvironmentVariable(geminiEnvVarName);
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[TomAIController] Loaded Gemini API key from environment variable '" + geminiEnvVarName + "'.");
            }
        }

        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = geminiApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[TomAIController] Using Gemini API key from Inspector (fallback).");
            }
        }

        bool hasApiKey = !string.IsNullOrEmpty(apiKey);
        Debug.Log($"[TomAIController] Gemini API key present: {hasApiKey}");
        if (hasApiKey)
        {
            request.SetRequestHeader("X-goog-api-key", apiKey);
        }
        else
        {
            Debug.LogWarning($"Gemini API key not set. Provide a key via Secrets/gemini_key.txt (Editor), environment variable '{geminiEnvVarName}', or Inspector field.");
        }

        Debug.Log($"[TomAIController] Sending request to: {geminiUrl}");
        Debug.Log($"[TomAIController] Request model: {geminiModel}");

        yield return request.SendWebRequest();

        Debug.Log($"[TomAIController] request.result={request.result}, responseCode={request.responseCode}, error={request.error}");
        string text = request.downloadHandler != null ? request.downloadHandler.text : null;
        Debug.Log($"[TomAIController] response body: {text}");

        string finalResponse = null;

        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                finalResponse = ParseGeminiContent(text);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[TomAIController] ParseGeminiContent threw: " + ex.Message);
                finalResponse = null;
            }

            if (string.IsNullOrEmpty(finalResponse))
            {
                try
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(text, "\"(text|content|message|generatedText)\"\\s*:\\s*\"([^\"]{2,})\",");
                    if (matches != null && matches.Count > 0)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (System.Text.RegularExpressions.Match m in matches)
                        {
                            if (m.Groups.Count >= 3)
                            {
                                if (sb.Length > 0) sb.Append(" ");
                                sb.Append(m.Groups[2].Value);
                            }
                        }
                        finalResponse = sb.ToString();
                    }
                }
                catch { }

                if (string.IsNullOrEmpty(finalResponse))
                {
                    try
                    {
                        var any = System.Text.RegularExpressions.Regex.Matches(text, "\"([^\"]{20,})\"");
                        if (any != null && any.Count > 0)
                        {
                            string longest = "";
                            foreach (System.Text.RegularExpressions.Match m in any)
                            {
                                if (m.Groups.Count >= 2)
                                {
                                    string v = m.Groups[1].Value;
                                    if (v.Length > longest.Length) longest = v;
                                }
                            }
                            if (!string.IsNullOrEmpty(longest)) finalResponse = longest;
                        }
                    }
                    catch { }
                }

                if (string.IsNullOrEmpty(finalResponse))
                {
                    finalResponse = text;
                }
            }
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            if (dialogText != null) dialogText.text = "Fehler: " + request.error;
            if (request.responseCode == 403)
            {
                if (!hasApiKey)
                {
                    Debug.LogError("[TomAIController] 403 PERMISSION_DENIED and no API key was provided. Set the GEMINI API key.");
                }
                else
                {
                    Debug.Log("[TomAIController] Received 403 Forbidden despite API key; retrying with key as query param.");
                    string urlWithKey = geminiUrl + (geminiUrl.Contains("?") ? "&" : "?") + "key=" + System.Uri.EscapeDataString(apiKey);

                    var retryReq = new UnityWebRequest(urlWithKey, "POST");
                    retryReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    retryReq.downloadHandler = new DownloadHandlerBuffer();
                    retryReq.SetRequestHeader("Content-Type", "application/json");

                    yield return retryReq.SendWebRequest();
                    Debug.Log($"[TomAIController] retry.request.result={retryReq.result}, responseCode={retryReq.responseCode}, error={retryReq.error}");
                    string retryText = retryReq.downloadHandler != null ? retryReq.downloadHandler.text : null;
                    Debug.Log($"[TomAIController] retry response body: {retryText}");

                    if (!string.IsNullOrEmpty(retryText))
                    {
                        try { finalResponse = ParseGeminiContent(retryText); } catch { finalResponse = null; }
                    }

                    if (retryReq.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(finalResponse))
                    {
                        dialog = new Dialog();
                        finalResponse = UnescapeCommonSequences(finalResponse);
                        var lines = SplitTextIntoPages(finalResponse, 25);
                        typeof(Dialog).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .SetValue(dialog, lines);
                        currentLine = 0;
                        if (dialogText != null) dialogText.text = lines[0];
                        isWaitingForResponse = false;
                        yield break;
                    }
                    else
                    {
                        Debug.LogError("[TomAIController] Retry with API key (query param) failed or returned no content.");
                    }
                }
            }
        }
        else if (string.IsNullOrEmpty(finalResponse))
        {
            if (!string.IsNullOrEmpty(text))
            {
                var pretty = UnescapeCommonSequences(text);
                if (dialogText != null) dialogText.text = pretty.Length > 1000 ? pretty.Substring(0, 1000) + "..." : pretty;
            }
            else
            {
                if (dialogText != null) dialogText.text = "Keine Antwort erhalten 😕";
            }
        }
        else
        {
            dialog = new Dialog();
            finalResponse = UnescapeCommonSequences(finalResponse);
            var lines = SplitTextIntoPages(finalResponse, 25);
            typeof(Dialog).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(dialog, lines);

            currentLine = 0;
            if (dialogText != null) dialogText.text = lines[0];
        }

        isWaitingForResponse = false;
    }

    private string ParseGeminiContent(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            var m = System.Text.RegularExpressions.Regex.Match(json, "\"text\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"generatedText\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"message\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"content\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"parts\"\\s*:\\s*\\[.*?\"text\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"candidates\"\\s*:\\s*\\[.*?\"content\"\\s*:\\s*\\[.*?\"text\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            var matches = System.Text.RegularExpressions.Regex.Matches(json, "\"(text|content|message|generatedText)\"\\s*:\\s*\"([^\"]{2,})\"");
            if (matches != null && matches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (System.Text.RegularExpressions.Match mm in matches)
                {
                    if (mm.Groups.Count >= 3)
                    {
                        if (sb.Length > 0) sb.Append(" ");
                        sb.Append(mm.Groups[2].Value);
                    }
                }
                return sb.ToString();
            }
        }
        catch { }

        return null;
    }

    private string UnescapeCommonSequences(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        try
        {
            s = s.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\\r", "\n");
            s = s.Replace("\\t", "    ");
            s = s.Replace("\\\"", "\"");
            s = s.Replace("\\\\", "\\");
        }
        catch { }

        return s;
    }

    private List<string> SplitTextIntoPages(string text, int maxWordsPerPage = 25)
    {
        List<string> pages = new List<string>();
        if (string.IsNullOrEmpty(text))
            return pages;

        string[] words = text.Split(' ');
        StringBuilder currentPage = new StringBuilder();
        int wordCount = 0;

        foreach (var word in words)
        {
            currentPage.Append(word + " ");
            wordCount++;

            if (wordCount >= maxWordsPerPage)
            {
                pages.Add(currentPage.ToString().Trim());
                currentPage.Clear();
                wordCount = 0;
            }
        }

        if (currentPage.Length > 0)
            pages.Add(currentPage.ToString().Trim());

        return pages;
    }

    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            Vector3 p = Application.isPlaying ? waypoints[i] : transform.TransformPoint(waypoints[i]);
            Gizmos.DrawSphere(p, 0.12f);
            if (i < waypoints.Length - 1)
                Gizmos.DrawLine(p, Application.isPlaying ? waypoints[i + 1] : transform.TransformPoint(waypoints[i + 1]));
        }
    }
}
