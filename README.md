## Spiellogik und Ablauf

Das Spiel ist ein kleines Mystery-RPG mit folgenden zentralen Mechaniken und Ablauf:

### 1. Ziel
Der Spieler muss das Geheimnis der "Silent Bell" lüften, indem er mit NPCs spricht, Rätsel löst und Hinweise sammelt.

### 2. Hauptcharaktere
- **Tom**: Patrouilliert auf einer festen Route im Burghof. Gibt Hinweise und das Codewort weiter.
- **Max**: Wächter der Burg. Fragt nach dem Codewort und stellt ein Rätsel. Gibt Zugang zur Glocke, wenn das Rätsel gelöst wird.

### 3. Ablauf
1. **Peter sprechen:**
  - Vor/bei seinem Haus. Er erklärt die Situation und gibt den Start-Hinweis.
2. **Bernd treffen:**
  - Auf dem Weg zum Schloss. Bernd gibt kontextuelle Tipps (KI-gestützt) zum nächsten Schritt.
3. **Tom finden:**
  - Tom patrouilliert im Burghof in einer runden Route.
  - Sprich Tom an, um Hinweise oder das Codewort zu erhalten.
4. **Max ansprechen:**
  - Max nach dem Rätsel fragen.
5. **Rätsel lösen:**
  - Beantworte Max’ Rätsel korrekt (z.B. „turm“/„tower“).
6. **Glocke finden:**
  - Nach erfolgreichem Dialog wird die Glocke sichtbar und kann aufgenommen werden.
7. **Glocke platzieren:**
  - Bringe die Glocke zum Turm und platziere sie dort, um die Aufgabe zu vollenden.

### 4. Steuerung
- Bewegung: Pfeiltasten oder WASD
- Interagieren: Taste (z.B. E oder Enter, je nach Einstellung)
- Dialoge: Antworten per Tastatur eingeben

### 5. KI-Dialog
- Die NPCs nutzen eine KI (Google Gemini API), um auf Eingaben des Spielers zu reagieren.
- Antworten werden im Spiel als Text angezeigt.

### 6. Fortschritt
- Der QuestManager merkt sich, mit wem gesprochen wurde und ob das Rätsel gelöst ist.
- Die Spielfortschritte werden im UI angezeigt.

---

Das Spiel kann beliebig erweitert werden, z.B. mit weiteren Rätseln, neuen NPCs oder zusätzlichen Quests.

## Welt & NPCs

Diese Sektion beschreibt die Spielwelt, wichtige Orte und wie die NPCs eingebunden sind.

### Orte
- **Dorf / Haus von Peter**: Einstiegsbereich. Hier steht Peter vor/seinem Haus und gibt erste Hinweise.
- **Weg zum Schloss**: Auf halber Strecke sitzt Bernd. Er reagiert auf Fragen über die KI und gibt kontextuelle Tipps.
- **Burghof**: Hier patrouilliert Tom auf einer festen Route und kann nach dem Aufenthaltsort der Glocke gefragt werden.
- **Schloss-Tor**: Max bewacht den Zugang zur Glocke. Er prüft das Codewort und stellt ein Rätsel.
- **Turm**: Zielort, an dem die Glocke letztlich platziert werden muss.

### NPCs und Interaktionen
- **Peter (am Haus)**
  - Rolle: Einführung und Weltkontext. Er erklärt, dass die Glocke verstummt ist und es Hinweise in Richtung Schloss gibt.
  - Interaktion: Kurze Dialoge ohne Rätsel; gibt einen „Start-Hinweis“ (z.B. „Sprich mit Tom oder Bernd auf dem Weg“).

- **Bernd (auf dem Weg, KI-gestützt)**
  - Rolle: Kontextuelle Tipps unterwegs; reagiert flexibel via Gemini-API.
  - Interaktion: Freitext-Fragen sind möglich (z.B. „Was soll ich als Nächstes tun?“). Bernd kann auf Spielfortschritt eingehen und Hinweise verdichten.
  - Technik: Gesteuert über `BerndAIController` (Assets/Scripts/BerndAIController.cs) mit Gemini-Integration analog zu Max.

- **Tom (patrouilliert im Burghof, KI-gestützt)**
  - Rolle: Gibt Hinweise, wo die Glocke sein könnte. Kann auch vor Gefahren warnen (flee-Mechanik bei Annäherung).
  - Patrouille: Läuft eine runde Route im Burghof (kreist um das Areal).
  - Interaktion: Freitext-Fragen sind möglich (z.B. „Wo ist die Glocke?“). Tom gibt Richtungs- oder Orts-Hinweise.
  - Technik: `TomAIController` (Assets/Scripts/TomAIController.cs). Patrouille + KI-Dialog.

- **Max (am Schloss, KI-gestützt)**
  - Rolle: Wächter. Prüft das Codewort und stellt ein Rätsel. Erst danach wird die Glocke freigegeben.
  - Interaktion: Codewort eingeben („sturmglocke“). Rätsel lösen (Antwort u.a. „turm“/„tower“). Danach wird die Glocke im Level sichtbar/erreichbar.
  - Technik: `MaxAIController` (Assets/Scripts/MaxAIController.cs) mit Gemini-Integration.

### Quest-Ziel: Glocke finden und am Turm platzieren
1. Mit Peter sprechen, um die Mission zu verstehen.
2. Auf dem Weg mit Bernd sprechen, um Tipps zu erhalten (frei formulierbar dank KI).
3. Im Burghof Tom ansprechen, um Hinweise zum Standort der Glocke zu bekommen (Tom patrouilliert).
4. Vor dem Schloss Max ansprechen: Codewort nennen und Rätsel lösen.
5. Glocke wird freigegeben/enthüllt; der Spieler kann sie aufnehmen.
6. Glocke zum Turm bringen und dort platzieren, um die Aufgabe zu vollenden.

### Fortschritt & UI
- Der `QuestManager` merkt sich, ob mit Tom/Max gesprochen wurde und ob das Rätsel gelöst ist.
- Die `QuestProgressUI` zeigt den Stand an (z.B. „Tom gesprochen“, „Max gesprochen“, „Glocke sichtbar“).

### Hinweise zur Umsetzung (Editor)
- Stelle sicher, dass die NPC-GameObjects in der Szene sind und die passenden Controller-Komponenten besitzen.
- Für KI-NPCs (Bernd, Tom, Max): Gemini-API-Key wie oben beschrieben setzen (Umgebungsvariable bevorzugt).
- Tom nutzt Wegpunkte; diese sind im `TomAIController` hinterlegt und können im Inspector überschrieben werden.
# RPG-Game - Mystery Game "The Silent Bell"

Short README with local development notes for AI integration.

## Overview
This Unity project is a small RPG/mystery game. The `MaxAIController` component (Assets/Scripts/MaxAIController.cs) implements an in-game chat dialog that can send prompts to an LLM backend and display paginated responses in-game.

## AI Integration
This project now uses the Google Generative (Gemini) REST API for in-game AI responses.

- Endpoint: `https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent` (configurable)
  - Default model used in code: `gemini-2.0-flash`

`MaxAIController` reads an API key from the Inspector field `geminiApiKey` and — if empty — falls back to the environment variable `GEMINI_API_KEY` by default.

### How to use Gemini (Google Generative API)
1. Set the environment variable in PowerShell for the current session (works if you start Unity from the same shell):

```powershell
$env:GEMINI_API_KEY="key"
```

2. Or persist the variable (Windows) so it is available after restarting Unity (requires restarting Unity):

```powershell
setx GEMINI_API_KEY "key"
```

3. In the Unity Editor:
- Select the GameObject with the `MaxAIController` component.
- Ensure `Gemini Env Var Name` is `GEMINI_API_KEY` (or change if you prefer another name).
- Either leave `Gemini Api Key` blank (env var used) or paste the key into that field for quick testing.

4. Example curl request (from project owner):

```bash
curl "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent" \
  -H 'Content-Type: application/json' \
  -H 'X-goog-api-key: here key' \
  -X POST \
  -d '{"contents":[{"parts":[{"text":"Explain how AI works in a few words"}]}]}'
```

5. Play the scene and interact with Max to send prompts.

### Inspector fields in `MaxAIController`
- `Gemini Api Key` (string): Optional; if set, used instead of environment variable.
- `Gemini Env Var Name` (string): Name of env var to read the key from (default: `GEMINI_API_KEY`).
- `Gemini Model` (string): The model name to send to the Generative API (default: `gemini-2.0-flash`).
- `Gemini Url` (string): Full endpoint to call for generateContent (default: `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent`).

## Security & best practices
- Never commit API keys or secrets into the repository. Use environment variables, CI secret stores, or Editor-only ignored files.
- The repository currently supports reading an OpenRouter API key from an environment variable; prefer this for local development and CI.
- If you temporarily paste a key into the Inspector for testing, make sure that you do not save the value into any committed Prefab/Scene that will store the secret.

Important Google/Gemini notes:
- Ensure the Generative API is enabled for your Google Cloud project and that billing is active — otherwise requests will return HTTP 403 Forbidden.
- Check API key restrictions in the Google Cloud Console: if the API key is restricted by HTTP referrers or IP addresses it can cause 403 errors from Unity. For local testing, use a key without restrictive referrers or add appropriate restrictions (IP/HTTP) that include your client.
- If you see `403 Forbidden` responses, check the response body in Unity Console (it often contains a diagnostic message) and verify the key/project configuration in https://console.cloud.google.com/

## Troubleshooting
- If you see a Unity Console warning that the API key is missing, confirm the Inspector field or that the environment variable is set and Unity was restarted (for `setx` changes).
- If using Ollama locally, confirm the Ollama process is running and reachable at the configured `Ollama Url`.

## Next steps (optional)
- Add an Editor-only secret store (ignored file) or use `System.Environment.GetEnvironmentVariable` from a secure CI context.
- Add unit tests / integration tests for request/response parsing (currently the code uses Unity's `JsonUtility` for a typical OpenRouter/OpenAI response and a fallback parser for Ollama streaming format).

---
If you want, I can also add a small `CONTRIBUTING.md` snippet that describes how to add keys to your development environment safely, or implement Editor-only secure storage for the key. Please tell me which you prefer.