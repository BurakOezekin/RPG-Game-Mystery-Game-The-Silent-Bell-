# The Silent Bell - Quest System Setup Guide

## Quest Overview (Die stille Glocke)

Diese Quest führt den Spieler durch die Stadt, um die verschwundene Glocke zu finden:

1. **Peter (am Haus)** - Startet die Quest
2. **Bernd (im Osten)** - Gibt einen FALSCHEN Hinweis (Westen/Brunnen) mit Gemini AI
3. **Tom (auf dem Weg)** - Läuft weg vom Spieler, gibt Rätsel-Hinweis zur Burg
4. **Max (bei der Burg)** - Enthüllt die Glocke mit dramatischer Gemini AI Geschichte
5. **Glocke** - Wird sichtbar und kann eingesammelt werden

---

## Unity Inspector Setup - Schritt für Schritt

### 1. QuestManager Setup

1. **Erstelle ein leeres GameObject in der Szene:**
   - Rechtsklick in Hierarchy → Create Empty
   - Benenne es: `QuestManager`

2. **Füge das QuestManager Script hinzu:**
   - Ziehe `QuestManager.cs` auf das GameObject
   - ODER: Add Component → QuestManager

3. **Keine weiteren Einstellungen nötig** - QuestManager ist ein Singleton

---

### 2. Peter (Quest Start NPC)

**Position:** Am Haus (House)

#### Inspector Einstellungen:
1. **PeterController Component:**
   - **Dialog:** Erstelle ein neues Dialog ScriptableObject:
     - Assets → Create → Dialog
     - Name: `PeterQuestDialog`
     - **Lines (Zeilen):**
       ```
       Willkommen in unserer Stadt!
       Wie du hörst, ertönt die Glocke nicht.
       Kannst du uns helfen, die Stadt zurück zu erobern?
       Auf dem Weg wirst du Bernd sehen.
       Frag ihn, wo die Glocke sein kann - er wird dir weiterhelfen.
       ```
   - Ziehe das Dialog Asset in das **Dialog** Feld von PeterController

2. **Interactable:** 
   - Stelle sicher, dass Peter das `Interactable` Interface implementiert
   - Collider2D muss auf "Is Trigger" gesetzt sein

---

### 3. Bernd (Falscher Hinweis mit Gemini AI)

**Position:** Im Osten der Stadt

#### Inspector Einstellungen:
1. **BerndAIController Component:**
   - **Dialog Box:** UI GameObject mit dem Dialog Panel
   - **Dialog Text:** Text Component für die Antwort
   - **Input Field:** InputField für Spielerfragen
   - **Dialog:** (Optional - wird dynamisch erstellt)

2. **API Configuration:**
   - **OpenRouter Api Key:** (Leer lassen - verwende Secrets Datei!)
   - **OpenRouter Model:** `google/gemma-3n-e2b-it:free`
   - **OpenRouter Url:** `https://openrouter.ai/api/v1/chat/completions`
   - **OpenRouter Env Var Name:** `OPENROUTER_API_KEY`

3. **OpenRouter API Key Setup (WICHTIG!):**
   - Erstelle Ordner im Projekt-Root: `Secrets/`
   - Erstelle Datei: `Secrets/openrouter_key.txt`
   - Füge deinen OpenRouter API Key ein
   - Diese Datei wird NICHT in Git committed!

#### Funktionsweise:
- Wenn nach "Glocke" gefragt: Gibt falschen Hinweis (Westen/Brunnen)
- Andere Fragen: Sagt "Das weiß ich leider nicht"

---

### 4. Tom (Flüchtender NPC mit Rätsel)

**Position:** Auf dem Weg zwischen Stadt und Burg

#### Inspector Einstellungen:
1. **TomAIController Component:**
   - **Waypoints:** Array mit Wegpunkten
     ```
     [0]: X: -13, Y: 8, Z: 0
     [1]: X: -3, Y: 8, Z: 0
     [2]: X: -3, Y: 17, Z: 0
     ```
   - **Speed:** `2` (normale Bewegungsgeschwindigkeit)
   - **Wait Time:** `1.0` (Wartezeit an Wegpunkten)
   - **Start On Awake:** ✓ (Aktiviert)

2. **Flee Behavior:**
   - **Flee Distance:** `3` (Abstand zum Spieler, bei dem Tom wegläuft)
   - **Flee Speed:** `4` (Fluchtgeschwindigkeit - schneller als normal!)
   - **Player Transform:** Ziehe das Player GameObject hierher

3. **Dialog (wie Bernd):**
   - **Dialog Box:** UI GameObject
   - **Dialog Text:** Text Component
   - **Input Field:** InputField Component

4. **API Configuration:** (Gleich wie Bernd)

#### Funktionsweise:
- **Fluchtverhalten:** Läuft weg, wenn Spieler näher als `3` Einheiten kommt
- **Bei Interaktion:** Stoppt Flucht, öffnet Dialog
- **Gemini Antworten:**
  - Fragen nach Glocke/Hilfe: Gibt Hinweis zur Burg
  - Fragen nach Rätsel: Gibt Rätsel-Hinweis zur Burg
  - Andere Fragen: Spielerische Antworten mit subtilen Burg-Hinweisen

---

### 5. Max (Finale NPC - Enthüllt Glocke)

**Position:** Bei der Burg (neben der versteckten Glocke)

#### Inspector Einstellungen:
1. **MaxAIController Component:**
   - **Dialog Box:** UI GameObject
   - **Dialog Text:** Text Component
   - **Input Field:** InputField Component
   - **Dialog:** (Optional - wird dynamisch erstellt)

2. **API Configuration:** (Gleich wie Bernd/Tom)

3. **Bell Quest:**
   - **Bell Object:** ⚠️ WICHTIG! Ziehe das Glocken-GameObject hierher!
     - Dies ist das GameObject mit dem BellItem Script
     - Max wird dieses GameObject aktivieren, wenn die Glocke enthüllt wird

#### Funktionsweise:
- **Bei Fragen nach Glocke:**
  - Gemini erzählt dramatische Geschichte über die Glocke
  - Aktiviert das Bell Object (macht es sichtbar)
  - Markiert Quest als "bellRevealed"
- **Andere Fragen:** Geheimnisvolle, hilfreiche Antworten

---

### 6. Glocke (Bell Item)

**Position:** Neben Max bei der Burg (versteckt)

#### GameObject Setup:
1. **Erstelle GameObject:**
   - Name: `Bell`
   - Füge SpriteRenderer hinzu (Glocken-Sprite)
   - Füge BoxCollider2D hinzu
     - **Is Trigger:** ✓ (Aktiviert)

2. **BellItem Component hinzufügen:**
   - **Item Name:** `Bell`
   - **Visual Object:** (Optional) Falls du ein separates Child-Objekt für die Grafik hast

#### Funktionsweise:
- **Start:** Komplett unsichtbar (SpriteRenderer disabled, Collider disabled)
- **Nach Max Gespräch:** Wird sichtbar (bellRevealed = true)
- **Bei Interaktion:** Wird ins Inventar gelegt, Quest komplett!

---

## UI Setup (Dialog System)

### Canvas Setup:
1. **Erstelle Canvas** (falls nicht vorhanden):
   - UI → Canvas
   - Canvas Scaler: Scale With Screen Size

2. **Dialog Box:**
   - Panel mit Hintergrund
   - Benenne: `DialogBox`
   - **Wichtig:** Start Inactive! (Deaktiviert in der Szene)

3. **Dialog Box Children:**
   ```
   DialogBox (Panel)
   ├── DialogText (Text) - Zeigt AI Antworten
   └── InputField (InputField) - Spieler tippt Fragen
   ```

4. **Verknüpfungen:**
   - Ziehe `DialogBox` in alle AI Controller (Bernd, Tom, Max)
   - Ziehe `DialogText` in alle AI Controller
   - Ziehe `InputField` in alle AI Controller

---

## OpenRouter API Key Setup (Sicherheit!)

### Option 1: Secrets Datei (EMPFOHLEN für Editor)
1. Erstelle im Projekt-Root: `Secrets/openrouter_key.txt`
2. Füge deinen OpenRouter API Key ein (z.B. aus dem kostenlosen Modell `google/gemini-2.0-flash-exp:free`)
3. Füge zu `.gitignore` hinzu:
   ```
   Secrets/
   ```

### Option 2: Environment Variable (Production)
1. Setze System-Umgebungsvariable:
   - Windows: `setx OPENROUTER_API_KEY "dein-api-key"`
   - Mac/Linux: `export OPENROUTER_API_KEY="dein-api-key"`

### Option 3: Inspector (NICHT EMPFOHLEN)
- Nur für Tests! Niemals in Version Control committen!

---

## Quest Flow - Spielerfahrung

### 1. Quest Start (Peter)
```
Spieler spricht mit Peter
→ Dialog: "Willkommen... Glocke fehlt... Frag Bernd..."
→ Quest aktiviert
```

### 2. Falscher Hinweis (Bernd)
```
Spieler geht zu Bernd (Osten)
→ Spieler: "Wo ist die Glocke?"
→ Bernd (Gemini): "Ich habe gehört, sie könnte im Westen am Brunnen sein!"
→ (FALSCHER Hinweis!)
```

### 3. Flüchtender Tom
```
Spieler nähert sich Tom
→ Tom läuft weg!
→ Spieler muss ihn "fangen" zum Reden
→ Spieler: "Wo ist die Glocke?"
→ Tom (Gemini): "Lauf zur Burg! Dort wirst du Antworten finden!"
```

### 4. Max enthüllt die Glocke
```
Spieler geht zur Burg, spricht mit Max
→ Spieler: "Wo ist die Glocke?"
→ Max (Gemini): "Die Glocke wurde von mir beschützt... Sie erscheint jetzt!"
→ ✨ Glocke wird sichtbar! ✨
```

### 5. Quest Abschluss
```
Spieler interagiert mit Glocke
→ Glocke ins Inventar
→ "🔔 Bell ins Inventar gelegt! Quest abgeschlossen!"
```

---

## Testing Checklist

- [ ] QuestManager GameObject in Szene vorhanden
- [ ] Peter hat Dialog mit Quest-Text
- [ ] Bernd hat DialogBox, DialogText, InputField verknüpft
- [ ] Bernd hat Gemini API Key (Secrets Datei)
- [ ] Tom hat Waypoints konfiguriert
- [ ] Tom hat Player Transform Referenz
- [ ] Tom hat Flee Distance = 3, Flee Speed = 4
- [ ] Max hat Bell Object Referenz
- [ ] Bell GameObject startet deaktiviert/unsichtbar
- [ ] Bell hat BellItem Component
- [ ] Alle NPCs haben Collider2D (Is Trigger = true)
- [ ] Alle NPCs implementieren Interactable

---

## System Prompts Übersicht

### Bernd System Prompts:
```csharp
// Wenn nach Glocke gefragt:
"Du bist Bernd, ein NPC in einem RPG-Spiel. Der Spieler sucht die Glocke. 
Du sollst ihm einen FALSCHEN Hinweis geben, dass die Glocke im Westen am Brunnen ist. 
Antworte kurz und überzeugend auf Deutsch..."

// Andere Fragen:
"Du bist Bernd, ein NPC in einem RPG-Spiel. 
Du weißt nur etwas über die Glocke (die angeblich im Westen am Brunnen sein soll). 
Für alle anderen Fragen antworte kurz auf Deutsch mit 'Das weiß ich leider nicht'..."
```

### Tom System Prompts:
```csharp
// Glocke/Hilfe:
"Du bist Tom, ein NPC in einem RPG-Spiel. Der Spieler sucht die Glocke. 
Sage ihm, er soll zur BURG gehen..."

// Rätsel:
"Du bist Tom, ein NPC in einem RPG-Spiel. 
Gib dem Spieler ein kurzes Rätsel auf Deutsch, dessen Antwort zur BURG führt..."

// Andere Fragen:
"Du bist Tom, ein frecher NPC in einem RPG-Spiel. 
Für allgemeine Fragen antworte kurz und spielerisch auf Deutsch..."
```

### Max System Prompts:
```csharp
// Glocke (ENTHÜLLUNG):
"Du bist Max, der Wächter der Burg in einem RPG-Spiel. Der Spieler hat die Glocke gesucht. 
Du weißt, wo sie ist! Sie ist HIER BEI DIR, neben der Burg versteckt! 
Erzähle eine kurze, dramatische Geschichte auf Deutsch (3-4 Sätze)..."

// Andere Fragen:
"Du bist Max, der Wächter der Burg in einem RPG-Spiel. 
Antworte kurz und geheimnisvoll auf Deutsch..."
```

---

## Troubleshooting

### "Glocke wird nicht sichtbar"
- ✓ Prüfe: Ist Bell Object in MaxAIController verknüpft?
- ✓ Prüfe: Hat Spieler nach "Glocke" gefragt?
- ✓ Console Log: "🔔 Die Glocke wurde enthüllt!"

### "Tom läuft nicht weg"
- ✓ Prüfe: Player Transform Referenz gesetzt?
- ✓ Prüfe: Flee Distance (z.B. 3) und Flee Speed (z.B. 4) korrekt?
- ✓ Prüfe: Tom hat Rigidbody2D?

### "Gemini antwortet nicht"
- ✓ Prüfe: Secrets/gemini_key.txt existiert?
- ✓ Prüfe: API Key ist gültig?
- ✓ Console Log: "[BerndAIController] Gemini API key present: True"
- ✓ Check: Generative AI API in Google Cloud aktiviert?

### "Dialog öffnet nicht"
- ✓ Prüfe: DialogBox, DialogText, InputField alle verknüpft?
- ✓ Prüfe: DialogBox ist im Canvas?
- ✓ Prüfe: NPC hat Interactable Component?

---

## Debug Logs

Die Scripts loggen wichtige Events:
- `[QuestManager] Quest started: Player talked to Peter`
- `[QuestManager] Player talked to Bernd (false hint given)`
- `[QuestManager] Player talked to Tom`
- `[QuestManager] Player talked to Max`
- `[QuestManager] Bell has been revealed!`
- `[QuestManager] Bell collected! Quest complete!`

---

## Erweiterungen / Ideen

1. **Quest UI:** Zeige Quest-Fortschritt im UI
2. **Sound Effects:** Glocken-Sound beim Enthüllen
3. **Partikel-Effekte:** Magische Enthüllung der Glocke
4. **Alternative Endings:** Verschiedene Dialog-Pfade
5. **Mehr NPCs:** Zusätzliche falsche Hinweise
6. **Journal System:** Aufzeichnung der Hinweise

---

## Credits

- **Quest Design:** The Silent Bell Mystery
- **AI Integration:** Google Gemini API
- **Unity Version:** 2022.3+ (anpassbar)
