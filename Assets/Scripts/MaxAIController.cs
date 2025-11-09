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

    private bool isDialogOpen = false;
    private bool isWaitingForResponse = false;
    private int currentLine = 0;

    public void Interact()
    {
        if (!isDialogOpen)
        {
            dialogBox.SetActive(true);
            dialogText.text = "Hallo, ich bin Max! Frag mich etwas:";
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
                StartCoroutine(SendToOllama(prompt));
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

    IEnumerator SendToOllama(string prompt)
    {
        isWaitingForResponse = true;
        inputField.gameObject.SetActive(false);
        dialogText.text = "Max denkt nach...";

        var url = "http://localhost:11434/api/generate";
        string body = "{\"model\":\"gemma:2b\",\"prompt\":\"" + prompt.Replace("\"", "\\\"") + "\"}";
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            dialogText.text = "Fehler: " + request.error;
        }
        else
        {
            string text = request.downloadHandler.text;
            StringReader reader = new StringReader(text);
            StringBuilder responseBuilder = new StringBuilder();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int start = line.IndexOf("\"response\":\"");
                if (start != -1)
                {
                    start += 12;
                    int end = line.IndexOf("\"", start);
                    if (end > start)
                    {
                        string part = line.Substring(start, end - start);
                        responseBuilder.Append(part);
                    }
                }
            }

            string finalResponse = responseBuilder.ToString();

            if (string.IsNullOrEmpty(finalResponse))
            {
                dialogText.text = "Keine Antwort erhalten 😕";
            }
            else
            {
                // 🔹 Text automatisch in mehrere Seiten aufteilen
                dialog = new Dialog();
                var lines = SplitTextIntoPages(finalResponse, 25); // ca. 2-3 Zeilen pro Seite
                typeof(Dialog).GetField("lines", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(dialog, lines);

                currentLine = 0;
                dialogText.text = lines[0] ;
            }
        }

        isWaitingForResponse = false;
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
