# RPG-Spiel – Mystery Game „The Silent Bell“

Kurze README mit Hinweisen zur lokalen Entwicklung und KI‑Integration.

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

## Überblick
Dieses Unity-Projekt ist ein kleines RPG-/Mystery-Spiel. Die Komponente `MaxAIController` (Assets/Scripts/MaxAIController.cs) implementiert einen In‑Game‑Chat, der Prompts an ein LLM sendet und Antworten seitenweise im Spiel anzeigt.

## KI-Integration
Dieses Projekt nutzt die Google Generative (Gemini) REST API für KI-Antworten im Spiel.

- Endpoint: `https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent` (konfigurierbar)
  - Standardmodell im Code: `gemini-2.0-flash`

`MaxAIController` liest den API‑Key aus dem Inspector-Feld `geminiApiKey` und – falls leer – standardmäßig aus der Umgebungsvariablen `GEMINI_API_KEY`.

### Gemini (Google Generative API) verwenden
1. Umgebungsvariable in PowerShell für die aktuelle Sitzung setzen (funktioniert, wenn du Unity aus derselben Shell startest):

```powershell
$env:GEMINI_API_KEY="key"
```

2. Oder dauerhaft (Windows) setzen, sodass sie nach einem Neustart von Unity verfügbar ist (Unity danach neu starten):

```powershell
setx GEMINI_API_KEY "key"
```

3. Im Unity-Editor:
- Wähle das GameObject mit der Komponente `MaxAIController`.
- Stelle sicher, dass `Gemini Env Var Name` auf `GEMINI_API_KEY` steht (oder passe den Namen an).
- Lasse `Gemini Api Key` leer (Umgebungsvariable wird genutzt) oder trage für schnelles Testen den Key vorübergehend ein.

4. Beispiel-curl-Aufruf:

```bash
curl "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent" \
  -H 'Content-Type: application/json' \
  -H 'X-goog-api-key: key' \
  -X POST \
  -d '{"contents":[{"parts":[{"text":"Erkläre in wenigen Worten, wie KI funktioniert"}]}]}'
```

5. Spiele die Szene und interagiere mit Max, um Prompts zu senden.

### Inspector-Felder in `MaxAIController`
- `Gemini Api Key` (string): Optional; wenn gesetzt, wird er statt der Umgebungsvariable verwendet.
- `Gemini Env Var Name` (string): Name der Umgebungsvariablen (Standard: `GEMINI_API_KEY`).
- `Gemini Model` (string): Modellname für die Generative API (Standard: `gemini-2.0-flash`).
- `Gemini Url` (string): Vollständiger Endpunkt für generateContent (Standard: `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent`).

## Sicherheit & Best Practices
- Keine API-Keys oder Secrets ins Repository committen. Nutze Umgebungsvariablen, CI-Secret-Stores oder Editor‑only ignorierte Dateien.
- Das Repository unterstützt das Lesen eines OpenRouter-API-Keys aus einer Umgebungsvariablen; bevorzuge dies für lokale Entwicklung und CI.
- Wenn du testweise einen Key in den Inspector einfügst, speichere ihn nicht dauerhaft in Prefabs/Szenen, die committed werden.

Wichtige Google/Gemini-Hinweise:
- Stelle sicher, dass die Generative API in deinem Google-Cloud-Projekt aktiviert ist und Abrechnung aktiv ist – sonst gibt es HTTP 403 Forbidden.
- Prüfe Key-Einschränkungen in der Google Cloud Console: Starke Einschränkungen (Referrer/IP) können 403-Fehler in Unity auslösen. Für lokale Tests entweder ohne restriktive Referrer oder passende Ausnahmen konfigurieren.
- Bei `403 Forbidden`: Antworttext in der Unity-Konsole prüfen (oft mit Diagnose) und Projekt/Key-Konfiguration in https://console.cloud.google.com/ verifizieren.

## Fehlerbehebung
- Erscheint in Unity eine Warnung, dass der API‑Key fehlt, prüfe den Inspector oder die Umgebungsvariable und starte Unity ggf. neu (für `setx`).
- Wenn du lokal Ollama nutzt, stelle sicher, dass der Ollama‑Prozess läuft und unter der konfigurierten `Ollama Url` erreichbar ist.

## Nächste Schritte (optional)
- Editor‑only Secret-Store (ignorierte Datei) ergänzen oder `System.Environment.GetEnvironmentVariable` aus sicherem CI-Kontext verwenden.
- Unit-/Integrationstests für Request-/Response‑Parsing ergänzen (der Code nutzt Unitys `JsonUtility` für typische OpenRouter/OpenAI‑Antworten und einen Fallback‑Parser für Ollama‑Streaming).

---
Wenn du möchtest, kann ich eine kurze `CONTRIBUTING.md` ergänzen, die erklärt, wie Keys sicher für die Entwicklung gesetzt werden, oder eine Editor‑only‑Lösung zur sicheren Speicherung implementieren.