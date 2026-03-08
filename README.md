# DevBites

A .NET MAUI app that delivers short, engaging programming concepts in an infinite scrolling feed — similar to Instagram Reels but for developers. Content is generated on-demand by a large language model (LLM) and streamed into a live feed grouped by topic.

---

## Table of Contents

- [Features](#features)
- [Architecture Overview](#architecture-overview)
- [Project Structure](#project-structure)
- [Key Components](#key-components)
  - [Model](#model)
  - [Service Layer](#service-layer)
  - [ViewModel](#viewmodel)
  - [View](#view)
  - [Configuration](#configuration)
- [AI Provider Setup](#ai-provider-setup)
  - [Google Gemini (default)](#google-gemini-default)
  - [Ollama — local inference](#ollama--local-inference)
- [Getting Started](#getting-started)
- [Dependency Injection Map](#dependency-injection-map)

---

## Features

- Infinite scrolling feed powered by live AI-generated content
- Topic picker (`.NET`, `Python`, `OOP`, `SOLID`, `Design Patterns`, `Data Structures`)
- 10 concept cards fetched per request; next fetch pre-triggered at card 8
- Swappable AI backend — switch between **Google Gemini** and a **local Ollama** model via a single `.env` line
- Full MVVM with `CommunityToolkit.Mvvm` source generators
- Dependency injection wired through the standard MAUI host builder

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────┐
│                        .env file                        │
│        (embedded resource — loaded at startup)          │
└──────────────────────────┬──────────────────────────────┘
                           │ DotEnvLoader.Load()
                           ▼
┌─────────────────────────────────────────────────────────┐
│                     MauiProgram.cs                      │
│                 (composition root / DI)                 │
│                                                         │
│  AI_PROVIDER=gemini ──► GeminiChatClient                │
│  AI_PROVIDER=ollama ──► OllamaApiClient                 │
│                              │                          │
│                       IChatClient (MEAI)                │
└──────────────────────────┬──────────────────────────────┘
                           │
           ┌───────────────▼───────────────┐
           │        IAIContentService      │
           │        AIContentService       │
           │  • builds LLM prompt          │
           │  • deserializes JSON response │
           │    → List<ProgrammingReel>    │
           └───────────────┬───────────────┘
                           │
           ┌───────────────▼───────────────┐
           │         FeedViewModel         │
           │  • ObservableCollection<Reel> │
           │  • topic selection            │
           │  • infinite-scroll pagination │
           │  • cancellation on topic swap │
           └───────────────┬───────────────┘
                           │ BindingContext
           ┌───────────────▼───────────────┐
           │      FeedPage.xaml (View)     │
           │  • Picker (topic selector)    │
           │  • CollectionView (feed)      │
           │  • loading / error overlays   │
           └───────────────────────────────┘
```

---

## Project Structure

```
DevBites/
│
├── .env                    # Local secrets — git-ignored
├── .env.example            # Committed template for .env
├── .gitignore
│
├── Constants.cs            # Reads env vars; exposes typed config
├── DotEnvLoader.cs         # Parses embedded .env into Environment vars
├── MauiProgram.cs          # App entry point, DI composition root
│
├── Models/
│   └── ProgrammingReel.cs  # Strongly typed JSON response model
│
├── Services/
│   ├── IAIContentService.cs
│   └── AIContentService.cs # LLM prompt, HTTP call, JSON parsing
│
├── ViewModels/
│   └── FeedViewModel.cs    # State, commands, pagination logic
│
└── Views/
    ├── FeedPage.xaml       # Dark-themed infinite scroll feed UI
    └── FeedPage.xaml.cs    # Constructor injection of FeedViewModel
```

---

## Key Components

### Model

**`Models/ProgrammingReel.cs`**

The single data model for a concept card. Fields map 1-to-1 to what the LLM returns as JSON.

| Property | Description |
|---|---|
| `Title` | Short catchy headline |
| `Hook` | One attention-grabbing sentence |
| `Explanation` | 2–3 sentences explaining the concept |
| `CodeExample` | Concise code snippet (5–10 lines) |
| `KeyTakeaway` | The most important lesson |
| `Language` | Topic/language label for the badge |

`System.Text.Json` `[JsonPropertyName]` attributes map the camelCase JSON keys to PascalCase C# properties.

---

### Service Layer

**`Services/IAIContentService.cs`**

```csharp
Task<IReadOnlyList<ProgrammingReel>> GetReelsAsync(string topic, CancellationToken ct);
```

A single-method interface. Callers never depend on Gemini, Ollama, or any HTTP detail.

---

**`Services/AIContentService.cs`**

Concrete implementation. Responsibilities:

1. **Builds the prompt** — injects `topic` into a constant template that instructs the LLM to return a strict JSON array of exactly 10 items.
2. **Calls the LLM** — via `IChatClient.GetResponseAsync` from `Microsoft.Extensions.AI`.
3. **Extracts JSON** — strips any accidental markdown fences the model may add before deserializing.
4. **Deserializes** — `System.Text.Json` maps the response into `List<ProgrammingReel>`.

> The service is intentionally unaware of which LLM backend is active. It only holds an `IChatClient` — the abstraction from `Microsoft.Extensions.AI`.

---

### ViewModel

**`ViewModels/FeedViewModel.cs`** — inherits `ObservableObject` (CommunityToolkit.Mvvm)

| Member | Role |
|---|---|
| `Reels` | `ObservableCollection<ProgrammingReel>` bound to the feed |
| `Topics` | Static list of topics shown in the Picker |
| `SelectedTopic` | Two-way bound; changing it cancels in-flight requests and starts a fresh load |
| `IsLoading` | Drives the activity indicator |
| `IsInitialLoading` | `true` only when loading with an empty list — shows the full-screen overlay |
| `HasError` | `true` when `ErrorMessage` is set — shows the error bar |
| `LoadInitialCommand` | Triggered from `OnAppearing`; starts the first batch |
| `LoadMoreCommand` | Bound to `RemainingItemsThresholdReachedCommand` on the CollectionView |

**Pagination flow:**

```
OnAppearing
    └─► LoadInitialCommand
            └─► StartFreshLoadAsync()         ← cancels previous CTS, clears list
                    └─► FetchReelsAsync(token) ← sets IsLoading, appends results

User scrolls to card 8 of 10
    └─► LoadMoreCommand (RemainingItemsThreshold = 2)
            └─► FetchReelsAsync(token)         ← appends next 10 cards

User changes topic
    └─► OnSelectedTopicChanged
            └─► StartFreshLoadAsync()         ← cancels previous CTS, clears list
```

**Cancellation:** every `StartFreshLoadAsync` call cancels the previous `CancellationTokenSource` before creating a new one. This ensures that switching topics mid-flight silently discards stale results.

---

### View

**`Views/FeedPage.xaml`**

- `Shell.NavBarIsVisible="False"` — uses a custom header instead of Shell's nav bar.
- **Header row** — app title + `Picker` for topic selection.
- **CollectionView** — `RemainingItemsThreshold="2"` triggers `LoadMoreCommand` when 2 items remain (i.e., the user is at card 8 of 10).
- **Bottom status bar** — `ActivityIndicator` while loading more; error label when `HasError`.
- **Initial loading overlay** — full-screen spinner shown via `IsInitialLoading` while the very first batch loads.
- All bindings use `x:DataType` (compiled bindings) for compile-time safety and performance.

---

### Configuration

**`.env` (git-ignored, embedded as a resource)**

```dotenv
AI_PROVIDER=gemini          # gemini | ollama

GEMINI_API_KEY=...
GEMINI_MODEL=gemini-2.0-flash

OLLAMA_ENDPOINT=http://localhost:11434
OLLAMA_MODEL=llama3.2
```

**`DotEnvLoader.cs`**  
Reads the embedded `.env` resource at startup and calls `Environment.SetEnvironmentVariable` for each key. Host environment variables take precedence (CI/CD-friendly).

**`Constants.cs`**  
Thin typed wrappers over `Environment.GetEnvironmentVariable`. `GeminiApiKey` throws a descriptive `InvalidOperationException` if the key is missing and Gemini is the active provider.

---

## AI Provider Setup

### Google Gemini (default)

1. Get a free API key from [https://aistudio.google.com/app/apikey](https://aistudio.google.com/app/apikey).
2. Open `.env` and set:
   ```dotenv
   AI_PROVIDER=gemini
   GEMINI_API_KEY=your_key_here
   GEMINI_MODEL=gemini-2.0-flash
   ```

### Ollama — local inference

1. Install Ollama from [https://ollama.com](https://ollama.com).
2. Pull the model:
   ```bash
   ollama pull llama3.2
   ```
3. Open `.env` and set:
   ```dotenv
   AI_PROVIDER=ollama
   OLLAMA_ENDPOINT=http://localhost:11434
   OLLAMA_MODEL=llama3.2
   ```

No code changes are required to switch providers — only the `.env` value changes.

---

## Getting Started

```bash
# 1. Clone the repo
git clone https://github.com/your-org/devbites.git
cd devbites

# 2. Create your local .env from the template
cp .env.example .env

# 3. Add your Gemini API key (or configure Ollama — see above)
#    Edit .env and set GEMINI_API_KEY=...

# 4. Run on Android
dotnet build -t:Run -f net10.0-android

# 5. Run on Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

---

## Dependency Injection Map

| Registered as | Concrete type | Lifetime |
|---|---|---|
| `IChatClient` | `GeminiChatClient` or `OllamaApiClient` | Singleton |
| `IAIContentService` | `AIContentService` | Singleton |
| `FeedViewModel` | `FeedViewModel` | Transient |
| `FeedPage` | `FeedPage` | Transient |
