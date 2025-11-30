# Die stille Glocke - Quest System Zusammenfassung

## ✅ Was wurde implementiert?

### 1. **QuestManager.cs** - Zentrale Quest-Verwaltung
- Singleton Pattern für globalen Zugriff
- Verfolgt alle NPC-Gespräche
- Verwaltet Glocken-Status (versteckt → enthüllt → eingesammelt)

### 2. **PeterController.cs** - Quest-Start
- Zeigt initialen Dialog: Willkommen, Glocke fehlt, frag Bernd
- Markiert Quest als gestartet

### 3. **BerndAIController.cs** - Falscher Hinweis mit Gemini
- **Gemini System Prompt:** 
  - Bei Fragen zur Glocke: Gibt FALSCHEN Hinweis (Westen/Brunnen)
  - Andere Fragen: "Das weiß ich leider nicht"
- Verwendet Gemini API für dynamische Antworten

### 4. **TomAIController.cs** - Flüchtender NPC mit Rätsel
- **Fluchtverhalten:** Läuft weg wenn Spieler näher als 3 Einheiten kommt
- **Gemini System Prompt:**
  - Glocke-Fragen: Hinweis zur BURG
  - Rätsel-Fragen: Gibt Rätsel mit Burg-Lösung
  - Andere Fragen: Spielerisch mit subtilen Hinweisen
- Patrol-System: Läuft zwischen Wegpunkten

### 5. **MaxAIController.cs** - Finale Enthüllung
- **Gemini System Prompt:**
  - Glocke-Fragen: Dramatische Geschichte + ENTHÜLLT die Glocke
  - Andere Fragen: Geheimnisvoll und hilfreich
- Aktiviert Bell GameObject wenn Glocke enthüllt wird

### 6. **BellItem.cs** - Intelligente Glocke
- Startet UNSICHTBAR (disabled SpriteRenderer + Collider)
- Wird sichtbar wenn Max die Glocke enthüllt
- Kann erst dann eingesammelt werden
- Markiert Quest als abgeschlossen

---

## 🎮 Quest-Ablauf

```
1. Peter (Haus)
   ↓ "Frag Bernd im Osten"
   
2. Bernd (Osten)
   ↓ "Glocke ist im Westen am Brunnen" (FALSCH!)
   
3. Tom (unterwegs)
   → Läuft weg!
   ↓ "Geh zur Burg!"
   
4. Max (Burg)
   ↓ Enthüllt Glocke mit dramatischer Story
   ✨ GLOCKE ERSCHEINT! ✨
   
5. Glocke aufheben
   🔔 QUEST ABGESCHLOSSEN!
```

---

## 🔧 Unity Inspector Setup - Wichtigste Punkte

### QuestManager
- Leeres GameObject in Szene
- QuestManager.cs Component

### Peter
- **Dialog:** ScriptableObject mit Quest-Text erstellen

### Bernd, Tom, Max (AI NPCs)
- **Dialog Box:** UI Panel Referenz
- **Dialog Text:** Text Component Referenz  
- **Input Field:** InputField Component Referenz
- **Gemini API Key:** Über `Secrets/gemini_key.txt` Datei!

### Tom (zusätzlich)
- **Waypoints:** Patrol-Wegpunkte
- **Flee Distance:** 3 (wann wegrennen)
- **Flee Speed:** 4 (schneller als normal)
- **Player Transform:** Spieler-Referenz

### Max (zusätzlich)
- **Bell Object:** ⚠️ WICHTIG! Glocken-GameObject verknüpfen!

### Bell (GameObject)
- **BellItem.cs** Component
- **Item Name:** "Bell"
- **Collider2D:** Is Trigger = ✓
- Startet DEAKTIVIERT in der Szene!

---

## 🔑 Gemini API Key Setup

### EMPFOHLEN: Secrets Datei
```
1. Erstelle: Secrets/gemini_key.txt im Projekt-Root
2. Füge API Key ein
3. In .gitignore: Secrets/
```

### Alternative: Umgebungsvariable
```powershell
setx GEMINI_API_KEY "dein-api-key-hier"
```

---

## 🎯 System Prompts - Übersicht

### Bernd (Falscher Hinweis)
```
GLOCKE-FRAGE:
"Du sollst einen FALSCHEN Hinweis geben: 
Glocke ist im Westen am Brunnen"

ANDERE FRAGEN:
"Antworte: Das weiß ich leider nicht"
```

### Tom (Rätsel zur Burg)
```
GLOCKE-FRAGE:
"Sage: Geh zur BURG"

RÄTSEL-FRAGE:
"Gib Rätsel dessen Antwort zur Burg führt"
```

### Max (Enthüllung)
```
GLOCKE-FRAGE:
"Erzähle dramatische Geschichte - 
Glocke war hier versteckt und erscheint jetzt!"
→ AKTIVIERT GLOCKEN-OBJEKT

ANDERE FRAGEN:
"Sei geheimnisvoll und hilfreich"
```

---

## 📝 Checkliste für Unity Setup

### Szenen-Objekte
- [ ] QuestManager GameObject mit Script
- [ ] Peter mit PeterController + Dialog
- [ ] Bernd mit BerndAIController + UI-Referenzen
- [ ] Tom mit TomAIController + Waypoints + Player Ref
- [ ] Max mit MaxAIController + Bell Object Ref
- [ ] Bell GameObject mit BellItem (startet deaktiviert!)

### UI Setup
- [ ] Canvas mit DialogBox (Panel)
- [ ] DialogText (Text Component)
- [ ] InputField (InputField Component)
- [ ] DialogBox startet DEAKTIVIERT

### API Setup
- [ ] Secrets/gemini_key.txt mit API Key
- [ ] Oder GEMINI_API_KEY Umgebungsvariable

### NPC Setup
- [ ] Alle NPCs haben Collider2D (Is Trigger = true)
- [ ] Alle NPCs implementieren Interactable
- [ ] Dialog-Referenzen alle korrekt verknüpft

---

## 🐛 Häufige Probleme & Lösungen

### Problem: "Glocke wird nicht sichtbar"
**Lösung:**
- Prüfe Bell Object Referenz in MaxAIController
- Prüfe ob Spieler nach "Glocke" gefragt hat
- Console Log prüfen: "Die Glocke wurde enthüllt!"

### Problem: "Tom läuft nicht weg"
**Lösung:**
- Player Transform Referenz setzen
- Flee Distance/Speed prüfen (3 / 4)
- Rigidbody2D auf Tom prüfen

### Problem: "Gemini antwortet nicht"
**Lösung:**
- Secrets/gemini_key.txt Datei prüfen
- API Key gültig?
- Google Cloud: Generative AI API aktiviert?
- Console Logs prüfen

### Problem: "Dialog öffnet nicht"
**Lösung:**
- Alle UI-Referenzen gesetzt?
- DialogBox im Canvas?
- NPC hat Interactable Component?

---

## 🎨 Erweiterungs-Ideen

1. **Visuelle Effekte:**
   - Partikel-System bei Glocken-Enthüllung
   - Glüh-Effekt auf der Glocke
   - Magischer "Erscheinungs-Effekt"

2. **Audio:**
   - Glocken-Sound bei Enthüllung
   - Ambient Sound für NPCs
   - Quest-Abschluss-Musik

3. **UI Verbesserungen:**
   - Quest-Log mit Fortschritt
   - Mini-Map mit NPC-Markierungen
   - Dialoghistorie

4. **Gameplay:**
   - Mehrere falsche Hinweise von verschiedenen NPCs
   - Tom-Fangen als Mini-Game
   - Alternative Quest-Pfade

5. **Narrative:**
   - Backstory für jeden NPC
   - Unterschiedliche Endings
   - Moralische Entscheidungen

---

## 📊 Debug Logs zum Testen

Console zeigt an:
```
[QuestManager] Quest started: Player talked to Peter
[QuestManager] Player talked to Bernd (false hint given)
[QuestManager] Player talked to Tom
[QuestManager] Player talked to Max
[QuestManager] Bell has been revealed!
[QuestManager] Bell collected! Quest complete!
```

---

## 💾 Neue Dateien

1. **QuestManager.cs** - Quest-Status-Verwaltung
2. **BellItem.cs** - Intelligente unsichtbare Glocke
3. **QUEST_SETUP_GUIDE.md** - Ausführliche Anleitung (Englisch)

## 🔄 Geänderte Dateien

1. **PeterController.cs** - Quest-Start Markierung
2. **BerndAIController.cs** - Falscher Hinweis System Prompt
3. **TomAIController.cs** - Flucht-Verhalten + Rätsel System Prompt
4. **MaxAIController.cs** - Glocken-Enthüllung System Prompt

---

## 🚀 Nächste Schritte

1. **Unity öffnen**
2. **Scripts kompilieren** (sollte automatisch passieren)
3. **Setup durchführen** (siehe QUEST_SETUP_GUIDE.md)
4. **Gemini API Key** in Secrets/gemini_key.txt
5. **Alle Referenzen** in Inspector setzen
6. **Testen!** 🎮

---

**Viel Erfolg mit deinem Quest System! 🔔**
