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