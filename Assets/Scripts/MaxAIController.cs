using System.Collections;
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

    private bool isDialogOpen = false;
    private bool isWaitingForResponse = false;

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

            PlayerController.canMove = false; // 🔒 Spieler darf sich nicht bewegen
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

        // ESC → Dialog schließen + Bewegung wieder aktivieren
        if (isDialogOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.SetActive(false);
            isDialogOpen = false;
            PlayerController.canMove = true; // 🔓 Spieler darf sich wieder bewegen
        }
    }

    IEnumerator SendToOllama(string prompt)
    {
        isWaitingForResponse = true;
        inputField.gameObject.SetActive(false); // Eingabe ausblenden
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
                        dialogText.text = responseBuilder.ToString();
                        yield return new WaitForSeconds(0.02f); // Text-Effekt
                    }
                }
            }

            string finalResponse = responseBuilder.ToString();
            if (string.IsNullOrEmpty(finalResponse))
                dialogText.text = "Keine Antwort erhalten 😕";
            else
                dialogText.text = finalResponse;
        }

        // Hinweis einblenden
        dialogText.text += "\n\n(Drücke Enter, um weiterzumachen...)";

        // Warte auf Enter → Eingabefeld wieder anzeigen
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return));

        inputField.gameObject.SetActive(true);
        inputField.ActivateInputField();

        isWaitingForResponse = false;
        // ❌ Kein PlayerController.canMove = true hier!
        // Bewegung bleibt gesperrt, bis Spieler mit ESC den Dialog verlässt
    }
}
