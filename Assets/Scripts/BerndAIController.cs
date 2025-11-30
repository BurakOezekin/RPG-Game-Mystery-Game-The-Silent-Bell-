using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

public class BerndAIController : MonoBehaviour, Interactable
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

    private bool isDialogOpen = false;
    private bool isWaitingForResponse = false;
    private int currentLine = 0;

    public void Interact()
    {
        if (!isDialogOpen)
        {
            // Mark Bernd as talked to
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.MarkBerndTalked();
            }
            
            dialogBox.SetActive(true);
            dialogText.text = "Hallo, ich bin Bernd! Frag mich etwas:";
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
        dialogText.text = "Bernd denkt nach...";

        // Check if asking about the bell/Glocke - give FALSE hint to the west/fountain
        string lower = prompt.ToLower();
        if (lower.Contains("glocke") || lower.Contains("bell") || lower.Contains("wo"))
        {
            // Create system prompt for Gemini to give FALSE hint
            string systemPrompt = "Du bist Bernd, ein NPC in einem RPG-Spiel. Der Spieler sucht die Glocke. " +
                "Du sollst ihm einen FALSCHEN Hinweis geben, dass die Glocke im Westen am Brunnen ist. " +
                "Antworte kurz und überzeugend auf Deutsch, als würdest du wirklich helfen. " +
                "Sage etwas wie 'Ich habe gehört, die Glocke könnte im Westen beim Brunnen sein' oder ähnlich. " +
                "Maximal 2-3 Sätze. Frage des Spielers: ";
            
            prompt = systemPrompt + prompt;
        }
        else
        {
            // For other questions, make Bernd say he doesn't know
            string systemPrompt = "Du bist Bernd, ein NPC in einem RPG-Spiel. " +
                "Du weißt nur etwas über die Glocke (die angeblich im Westen am Brunnen sein soll). " +
                "Für alle anderen Fragen antworte kurz auf Deutsch mit 'Das weiß ich leider nicht' oder " +
                "'Tut mir leid, darüber kann ich dir nichts sagen' oder ähnlich. " +
                "Maximal 1-2 Sätze. Frage des Spielers: ";
            
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
                        Debug.Log("[BerndAIController] Loaded Gemini API key from Secrets/gemini_key.txt (Editor-only).");
                    }
                }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[BerndAIController] Failed reading Secrets/gemini_key.txt: " + ex.Message);
        }
#endif

        // Next: environment variable
        if (string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(geminiEnvVarName))
        {
            apiKey = System.Environment.GetEnvironmentVariable(geminiEnvVarName);
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[BerndAIController] Loaded Gemini API key from environment variable '" + geminiEnvVarName + "'.");
            }
        }

        // Fallback: Inspector field
        if (string.IsNullOrEmpty(apiKey))
        {
            apiKey = geminiApiKey;
            if (!string.IsNullOrEmpty(apiKey))
            {
                Debug.Log("[BerndAIController] Using Gemini API key from Inspector (fallback).");
            }
        }

        bool hasApiKey = !string.IsNullOrEmpty(apiKey);
        Debug.Log($"[BerndAIController] Gemini API key present: {hasApiKey}");
        if (hasApiKey)
        {
            request.SetRequestHeader("X-goog-api-key", apiKey);
        }
        else
        {
            Debug.LogWarning($"Bernd Gemini API key not set. Provide a key via Secrets/gemini_key.txt (Editor), environment variable '{geminiEnvVarName}', or Inspector field.");
        }

        // Debug: log request details
        Debug.Log($"[BerndAIController] Sending request to: {geminiUrl}");
        Debug.Log($"[BerndAIController] Request model: {geminiModel}");

        yield return request.SendWebRequest();

        // Always log result details to aid debugging networking issues
        Debug.Log($"[BerndAIController] request.result={request.result}, responseCode={request.responseCode}, error={request.error}");
        string text = request.downloadHandler != null ? request.downloadHandler.text : null;
        Debug.Log($"[BerndAIController] response body: {text}");

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
                Debug.LogWarning("[BerndAIController] ParseGeminiContent threw: " + ex.Message);
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
                    Debug.LogError("[BerndAIController] 403 PERMISSION_DENIED and no API key was provided. Set the GEMINI API key in the Inspector or as environment variable '" + geminiEnvVarName + "'.");
                    Debug.LogError("Also ensure the Generative API is enabled for your Google Cloud project and billing is active.");
                }
                else
                {
                    Debug.Log("[BerndAIController] Received 403 Forbidden despite API key being present; retrying with API key as query parameter (will not log the key)");

                    // build URL with key parameter (do not log the key)
                    string urlWithKey = geminiUrl + (geminiUrl.Contains("?") ? "&" : "?") + "key=" + System.Uri.EscapeDataString(apiKey);

                    var retryReq = new UnityWebRequest(urlWithKey, "POST");
                    retryReq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                    retryReq.downloadHandler = new DownloadHandlerBuffer();
                    retryReq.SetRequestHeader("Content-Type", "application/json");
                    // do not set X-goog-api-key header here (we use query param)

                    yield return retryReq.SendWebRequest();
                    Debug.Log($"[BerndAIController] retry.request.result={retryReq.result}, responseCode={retryReq.responseCode}, error={retryReq.error}");
                    string retryText = retryReq.downloadHandler != null ? retryReq.downloadHandler.text : null;
                    Debug.Log($"[BerndAIController] retry response body: {retryText}");

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
                        Debug.LogError("[BerndAIController] Retry with API key (query param) failed or returned no content. Check project/API key permissions, enable Generative API and billing, or review key restrictions.");
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
        }

        isWaitingForResponse = false;
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
