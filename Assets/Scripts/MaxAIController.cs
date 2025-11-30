using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class MaxAIController : MonoBehaviour, Interactable
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] InputField inputField;
    [SerializeField] Dialog dialog;

    [Header("API Configuration")]
    [Tooltip("Gemini API key. Prefer using the local Secrets/gemini_key.txt (Editor) or env var for production.")]
    [SerializeField] private string geminiApiKey = "";

    [Tooltip("Gemini model name (e.g. gemini-2.0-flash)")]
    [SerializeField] private string geminiModel = "gemini-2.0-flash";

    [Tooltip("Gemini endpoint (default: https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent)")]
    [SerializeField] private string geminiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    [Tooltip("Name of the environment variable to read the Gemini API key from (used when `geminiApiKey` is empty).")]
    [SerializeField] private string geminiEnvVarName = "GEMINI_API_KEY";

    [Header("Bell Quest")]
    [SerializeField] private GameObject bellObject; // Reference to the bell GameObject in the scene
    [SerializeField] private string secretCodeWord = "sturmglocke"; // Secret code from Tom
    [SerializeField] private string[] riddleAnswers = new string[] { "turm", "tower" }; // Acceptable answers (prefer Turm)
    
    private bool isDialogOpen = false;
    private bool isWaitingForResponse = false;
    private int currentLine = 0;
    private bool bellRevealed = false;
    private bool riddleAsked = false;
    private bool codeWordGiven = false;
    private int conversationStage = 0; // 0=need code, 1=code given, 2=riddle asked, 3=bell revealed

    public void Interact()
    {
        if (!isDialogOpen)
        {
            // Mark Max as talked to
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.MarkMaxTalked();
            }
            
            dialogBox.SetActive(true);
            
            // Different greeting based on conversation stage
            if (conversationStage == 0)
            {
                dialogText.text = "Ich bin Max, Wächter der Burg. Kennst du das Codewort?";
            }
            else if (conversationStage == 1)
            {
                dialogText.text = "Das Codewort ist korrekt. Frage mich nach der Glocke.";
            }
            else if (conversationStage == 2 && !bellRevealed)
            {
                dialogText.text = "Hast du die Antwort auf mein Rätsel?";
            }
            else if (bellRevealed)
            {
                dialogText.text = "Die Glocke wurde enthüllt. Nimm sie!";
            }
            
            inputField.text = "";
            inputField.gameObject.SetActive(true);
            inputField.ActivateInputField();
            isDialogOpen = true;

            PlayerController.canMove = false;
        }
    }

    private void Update()
    {
        // ENTER → Anfrage senden
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Return) && !isWaitingForResponse)
        {
            string prompt = inputField.text.Trim();
            if (prompt.Length > 0)
            {
                inputField.text = "";
                StartCoroutine(SendToApi(prompt));
            }
        }

        // 🔹 L → nächste Seite anzeigen
        if (isDialogOpen && Input.GetKeyDown(KeyCode.L) && !isWaitingForResponse)
        {
            if (dialog != null && dialog.Lines != null && dialog.Lines.Count > 0)
            {
                currentLine++;
                if (currentLine < dialog.Lines.Count)
                {
                    dialogText.text = dialog.Lines[currentLine] ;
                }
                else
                {
                    dialogText.text = "\n(Drücke ESC zum Schließen)";
                }
            }
        }

        // 🔹 ESC → schließen
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.SetActive(false);
            isDialogOpen = false;
            PlayerController.canMove = true;
        }
    }

    IEnumerator SendToApi(string prompt)
    {
        isWaitingForResponse = true;
        inputField.gameObject.SetActive(false);
        dialogText.text = "Max denkt nach...";

        string lower = prompt.ToLower().Trim();
        
        // Stage 0: First conversation - player asks about bell
        if (conversationStage == 0)
        {
            if (lower.Contains("glocke") || lower.Contains("bell") || lower.Contains("wo"))
            {
                // Player asks about bell -> Max gives riddle
                    string systemPrompt = "SYSTEM: Du bist Max, Wächter der Burg im mittelalterlichen RPG 'Die stille Glocke'. " +
                        "Regeln: 1) Du kennst nur die Spielwelt. 2) Wenn der Spieler nach der Glocke fragt, gib KEINE Hinweise zur Lösung, sondern fordere ihn auf, nach dem Rätsel zu fragen. " +
                        "Aufgabe: Antworte kurz (1-2 Sätze) auf Deutsch: 'Frag mich nach dem Rätsel, dann helfe ich dir weiter.' Bleibe geheimnisvoll. " +
                        "Eingabe des Spielers: ";
                
                prompt = systemPrompt + prompt;
                conversationStage = 1; // Move to riddle stage
            }
            else
            {
                // Other questions - only knows game world
                string systemPrompt = "Du bist Max, Wächter der Burg im mittelalterlichen RPG 'Die stille Glocke'. " +
                    "Du weißt NUR über diese Spielwelt: das Dorf, die Burg, die Glocke, die Wächter. " +
                    "Für Fragen über moderne Dinge (Auto, Computer, Internet, etc.) antworte: 'Davon habe ich noch nie gehört!' " +
                    "Für Spielwelt-Fragen antworte passend und kurz auf Deutsch (1-2 Sätze). " +
                    "Frage des Spielers: ";
                
                prompt = systemPrompt + prompt;
            }
        }
        // Stage 1: Player asks for riddle
        else if (conversationStage == 1)
        {
            if (lower.Contains("rätsel") || lower.Contains("riddle") || lower.Contains("puzzle"))
            {
                // Max gives a very simple riddle via Gemini (answer must be 'Turm')
                    string systemPrompt = "SYSTEM: Du bist Max, Wächter der Burg im mittelalterlichen RPG 'Die stille Glocke'. " +
                        "Regeln: 1) Du kennst nur die Spielwelt. 2) Gib EIN SEHR EINFACHES Rätsel (GENAU 1 Satz) auf Deutsch. 3) Die EINZIGE richtige Lösung ist 'Turm'. 4) Das Rätsel darf NICHT die Wörter 'Turm', 'Glocke' oder 'Antwort' enthalten. 5) VARIERE die Formulierung zwischen Aufrufen. 6) Schliesse mit: 'Nenne die Lösung.' " +
                        "Beispiel: 'Ich überrage das Dorf, aus Stein und Zeit, wo der Klang einst begann — wohin führt dein Weg? Nenne die Lösung.' " +
                        "Eingabe des Spielers: ";
                
                prompt = systemPrompt + prompt;
                conversationStage = 2;
                riddleAsked = true;
            }
            else
            {
                // Remind player to ask for riddle
                string systemPrompt = "Du bist Max, Wächter der Burg im mittelalterlichen RPG 'Die stille Glocke'. " +
                    "Der Spieler hat noch nicht nach dem Rätsel gefragt. " +
                    "Antworte auf Deutsch (1 Satz): 'Frage mich nach dem Rätsel wenn du bereit bist!' " +
                    "Oder für andere Spielwelt-Fragen antworte kurz. " +
                    "Für moderne Dinge sage: 'Davon habe ich noch nie gehört!' " +
                    "Frage des Spielers: ";
                
                prompt = systemPrompt + prompt;
            }
        }
        // Stage 2: Riddle asked - check answer
        else if (conversationStage == 2 && !bellRevealed)
        {
            // Check if player gave correct riddle answer (simple, trimmed)
            bool correctAnswer = false;
            string trimmed = lower.Replace(".", "").Replace("!", "").Replace("?", "").Trim();
            foreach (string answer in riddleAnswers)
            {
                if (trimmed == answer || trimmed.Contains(answer))
                {
                    correctAnswer = true;
                    break;
                }
            }
            
            if (correctAnswer)
            {
                // CORRECT! Answer is 'Turm' — instruct next step and reveal
                string systemPrompt = "Du bist Max, der Wächter der Burg in einem RPG-Spiel. " +
                    "Der Spieler hat dein Rätsel richtig gelöst: 'Turm'. " +
                    "Antworte auf Deutsch (2-3 Sätze): 'Ja, das ist richtig — der Turm! Bringe die Glocke dorthin und lass den Turm wieder ertönen.' " +
                    "Beschreibe kurz, dass die Glocke erscheint und nun zum Turm gebracht werden soll. " +
                    "Sei feierlich und klar! Antwort des Spielers war: ";
                
                prompt = systemPrompt + prompt;
                bellRevealed = true;
                conversationStage = 3;
                StartCoroutine(RevealBellAfterDelay(3f));
            }
            else
            {
                // Wrong answer - give a simple hint via Gemini (point to 'Turm' without saying 'Glocke')
                string systemPrompt = "SYSTEM: Du bist Max, Wächter der Burg. " +
                    "Regeln: 1) Gib einen sehr einfachen Hinweis (1 Satz) auf Deutsch. 2) Verrate die Antwort nicht. 3) Deute klar darauf, dass die Lösung ein Ort ist: der 'Turm'. 4) Nenne NICHT 'Glocke'. " +
                    "Beispiel: 'Denke an das hohe Bauwerk aus Stein über dem Dorf — den Turm.' " +
                    "Eingabe des Spielers: ";
                
                prompt = systemPrompt + prompt;
            }
        }
        // Stage 3: Bell revealed - final conversations
        else if (conversationStage == 3)
        {
            string systemPrompt = "Du bist Max, der Wächter der Burg in einem RPG-Spiel. " +
                "Der Spieler hat das Rätsel gelöst und die Glocke wurde enthüllt! " +
                "Du weißt NUR über diese Spielwelt: das Dorf, die Burg, die Glocke, die Wächter. " +
                "Für Fragen über moderne Dinge (Auto, Computer, Internet, etc.) antworte: 'Davon habe ich noch nie gehört!' " +
                "Für Spielwelt-Fragen antworte freundlich auf Deutsch (1-2 Sätze). " +
                "Frage des Spielers: ";
            
            prompt = systemPrompt + prompt;
        }

        // Build Gemini (Google Generative) request body
        string body = "{\"contents\":[{\"parts\":[{\"text\":\"" + prompt.Replace("\"", "\\\"") + "\"}]}]}";

        var request = new UnityWebRequest(geminiUrl, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Determine API key priority (safer workflow):
        // 1) Editor-only local secrets file: <project-root>/Secrets/gemini_key.txt
        // 2) Environment variable named by `geminiEnvVarName` (e.g. GEMINI_API_KEY)
        // 3) Inspector field `geminiApiKey` (fallback)
        string apiKey = null;

#if UNITY_EDITOR
        // Prefer a local secrets file when running in the Editor so keys are not stored in Inspector or source control.
        try
        {
            string secretsPath = Path.Combine(Application.dataPath, "..", "Secrets", "gemini_key.txt");
            if (File.Exists(secretsPath))
            {
                apiKey = File.ReadAllText(secretsPath).Trim();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    Debug.Log("[MaxAIController] Loaded Gemini API key from Secrets/gemini_key.txt (Editor-only).");
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[MaxAIController] Failed reading Secrets/gemini_key.txt: " + ex.Message);
        }
#endif

        // Next: environment variable
        if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(geminiEnvVarName))
        {
            apiKey = System.Environment.GetEnvironmentVariable(geminiEnvVarName);
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[MaxAIController] Loaded Gemini API key from environment variable '" + geminiEnvVarName + "'.");
            }
        }

        // Fallback: Inspector field
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = geminiApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[MaxAIController] Using Gemini API key from Inspector (fallback).");
            }
        }

        bool hasApiKey = !string.IsNullOrEmpty(apiKey);
        Debug.Log($"[MaxAIController] Gemini API key present: {hasApiKey}");
        if (hasApiKey)
        {
            request.SetRequestHeader("X-goog-api-key", apiKey);
        }
        else
        {
            Debug.LogWarning($"Gemini API key not set. Provide a key via Secrets/gemini_key.txt (Editor), environment variable '{geminiEnvVarName}', or Inspector field.");
        }

        // Debug: log request details
        Debug.Log($"[MaxAIController] Sending request to: {geminiUrl}");
        Debug.Log($"[MaxAIController] Request model: {geminiModel}");

        yield return request.SendWebRequest();

        // Always log result details to aid debugging networking issues
        Debug.Log($"[MaxAIController] request.result={request.result}, responseCode={request.responseCode}, error={request.error}");
        string text = request.downloadHandler != null ? request.downloadHandler.text : null;
        Debug.Log($"[MaxAIController] response body: {text}");

        string finalResponse = null;

        // Try to extract generated text from Gemini response
        if (!string.IsNullOrEmpty(text))
        {
            try
            {
                finalResponse = ParseGeminiContent(text);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[MaxAIController] ParseGeminiContent threw: " + ex.Message);
                finalResponse = null;
            }

            // If parsing returned nothing, try a best-effort fallback: collect common string fields or show raw JSON
            if (string.IsNullOrEmpty(finalResponse))
            {
                // Collect all likely text occurrences via regex (text, content, message, generatedText)
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
                catch { /* ignore regex failures */ }

                // Still empty? As a last resort, try to extract any long quoted strings (likely the reply)
                if (string.IsNullOrEmpty(finalResponse))
                {
                    try
                    {
                        var any = System.Text.RegularExpressions.Regex.Matches(text, "\"([^\"]{20,})\"");
                        if (any != null && any.Count > 0)
                        {
                            // pick the longest match
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

                // If still nothing, show the raw JSON (trimmed) so the user can see what arrived
                if (string.IsNullOrEmpty(finalResponse))
                {
                    finalResponse = text;
                }
            }
        }
        if (request.result != UnityWebRequest.Result.Success)
        {
            dialogText.text = "Fehler: " + request.error;
            // If we got a 403 Forbidden from Google, try sending the API key as a URL query parameter
            if (request.responseCode == 403)
            {
                if (!hasApiKey)
                {
                    Debug.LogError("[MaxAIController] 403 PERMISSION_DENIED and no API key was provided. Set the GEMINI API key in the Inspector or as environment variable '\"" + geminiEnvVarName + "\"'.");
                    Debug.LogError("Also ensure the Generative API is enabled for your Google Cloud project and billing is active.");
                }
                else
                {
                    Debug.Log("[MaxAIController] Received 403 Forbidden despite API key being present; retrying with API key as query parameter (will not log the key)");
                // build URL with key parameter (do not log the key)
                string urlWithKey = geminiUrl + (geminiUrl.Contains("?") ? "&" : "?") + "key=" + System.Uri.EscapeDataString(apiKey);

                var retryReq = new UnityWebRequest(urlWithKey, "POST");
                retryReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                retryReq.downloadHandler = new DownloadHandlerBuffer();
                retryReq.SetRequestHeader("Content-Type", "application/json");
                // do not set X-goog-api-key header here (we use query param)

                yield return retryReq.SendWebRequest();
                Debug.Log($"[MaxAIController] retry.request.result={retryReq.result}, responseCode={retryReq.responseCode}, error={retryReq.error}");
                string retryText = retryReq.downloadHandler != null ? retryReq.downloadHandler.text : null;
                Debug.Log($"[MaxAIController] retry response body: {retryText}");

                // Attempt to parse retry response
                if (!string.IsNullOrEmpty(retryText))
                {
                    try { finalResponse = ParseGeminiContent(retryText); } catch { finalResponse = null; }
                }

                if (retryReq.result == UnityWebRequest.Result.Success && !string.IsNullOrEmpty(finalResponse))
                {
                    // use retry result
                    dialog = new Dialog();
                    // make escaped newlines/readable sequences human-friendly
                    finalResponse = UnescapeCommonSequences(finalResponse);
                    var lines = SplitTextIntoPages(finalResponse, 25);
                    typeof(Dialog).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        .SetValue(dialog, lines);
                    currentLine = 0;
                    dialogText.text = lines[0];
                    isWaitingForResponse = false;
                    yield break;
                }
                else
                {
                    Debug.LogError("[MaxAIController] Retry with API key (query param) failed or returned no content. Check project/API key permissions, enable Generative API and billing, or review key restrictions.");
                }
            }
        }

        }
        else if (string.IsNullOrEmpty(finalResponse))
        {
            // If we couldn't parse any generated text but we received a response body, show it to help debugging
            if (!string.IsNullOrEmpty(text))
            {
                var pretty = UnescapeCommonSequences(text);
                dialogText.text = pretty.Length > 1000 ? pretty.Substring(0, 1000) + "..." : pretty;
            }
            else
            {
                dialogText.text = "Keine Antwort erhalten 😕";
            }
        }
        else
        {
            // 🔹 Text automatisch in mehrere Seiten aufteilen
            dialog = new Dialog();
            // Normalize escaped sequences so the dialog renders line breaks instead of literal \n
            finalResponse = UnescapeCommonSequences(finalResponse);
            var lines = SplitTextIntoPages(finalResponse, 25); // ca. 2-3 Zeilen pro Seite
            typeof(Dialog).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(dialog, lines);

            currentLine = 0;
            dialogText.text = lines[0];
            
            // Reveal the bell if this was a bell-related question
            if (bellRevealed)
            {
                RevealBell();
            }
        }

        isWaitingForResponse = false;
    }

    private void RevealBell()
    {
        if (bellObject != null)
        {
            // First, activate the GameObject if it was deactivated
            if (!bellObject.activeSelf)
            {
                bellObject.SetActive(true);
                Debug.Log("🔓 Bell GameObject activated!");
            }
            
            // Then trigger the reveal on the BellItem component
            BellItem bellItem = bellObject.GetComponent<BellItem>();
            if (bellItem != null)
            {
                bellItem.Reveal();
            }
            
            Debug.Log("🔔 Die Glocke wurde enthüllt!");
            
            // Mark in quest manager
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.RevealBell();
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Bell object reference is not set in MaxAIController!");
        }
    }

    IEnumerator RevealBellAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RevealBell();
    }

    // Parse Gemini/Generative API response (best-effort). We look for common fields like "text" or "content".
    private string ParseGeminiContent(string json)
    {
        if (string.IsNullOrEmpty(json))
            return null;
        try
        {
            // 1) Direct "text" fields
            var m = System.Text.RegularExpressions.Regex.Match(json, "\"text\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            // 2) generatedText / message / content
            m = System.Text.RegularExpressions.Regex.Match(json, "\"generatedText\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"message\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            m = System.Text.RegularExpressions.Regex.Match(json, "\"content\"\\s*:\\s*\"([^\"]+)\"");
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            // 3) parts -> text (multi-line / nested) (singleline mode)
            m = System.Text.RegularExpressions.Regex.Match(json, "\"parts\"\\s*:\\s*\\[.*?\"text\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            // 4) candidates/content nested formats
            m = System.Text.RegularExpressions.Regex.Match(json, "\"candidates\"\\s*:\\s*\\[.*?\"content\"\\s*:\\s*\\[.*?\"text\"\\s*:\\s*\"([^\"]+)\"", System.Text.RegularExpressions.RegexOptions.Singleline);
            if (m.Success && m.Groups.Count >= 2) return m.Groups[1].Value;

            // 5) As a fallback, collect multiple text-like fields and join them
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

    // Convert common escaped sequences from JSON (e.g. "\\n") into readable characters for the dialog
    private string UnescapeCommonSequences(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        try
        {
            // Replace CRLF / LF escapes with actual newlines
            s = s.Replace("\\r\\n", "\n").Replace("\\n", "\n").Replace("\\r", "\n");
            // Tabs -> spaces
            s = s.Replace("\\t", "    ");
            // Unescape escaped quotes
            s = s.Replace("\\\"", "\"");
            // Reduce double-escaped backslashes
            s = s.Replace("\\\\", "\\");
        }
        catch { }

        return s;
    }

    

    // 🔹 Verbesserte Funktion: teilt Text in sinnvolle Seiten
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

            // Wenn zu viele Wörter → Seite speichern
            if (wordCount >= maxWordsPerPage)
            {
                pages.Add(currentPage.ToString().Trim());
                currentPage.Clear();
                wordCount = 0;
            }
        }

        // Letzte Seite anhängen
        if (currentPage.Length > 0)
            pages.Add(currentPage.ToString().Trim());

        return pages;
    }
}
