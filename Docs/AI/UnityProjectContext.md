# Unity Project Context

<!-- unity-onboarding:generated:start -->

## Project Summary

- Project root: `C:\Users\divij\Documents\UnityProjects\Projects\FLIGHT_DGC_RTR_TEST`
- Last analyzed: 2026-10-13
- Last analyzed commit: `dbac8fd` (`feat:ATC-interaction-completed`, 2026-10-12); working tree had pre-existing edits and untracked files during analysis.
- Purpose: Unity-based DGCA RTR Part 2 flight radiotelephony practice. Users move through flight-phase questions, record spoken answers, receive evaluation, and hear ATC prompts.

## Confirmed Environment

- Unity version: 6000.3.18f1 (revision `5ebeb53e4c07`).
- Render pipeline: Universal Render Pipeline 17.3.0, configured in `ProjectSettings/GraphicsSettings.asset`.
- Input system: Input System package 1.19.0; `activeInputHandler: 1` selects the new system. First-party scripts inspected do not directly use Input System APIs.
- Target platforms: not established from the inspected settings. Native Whisper wrapper and bundled server launch scripts imply a desktop-oriented local setup; verify platform support before targeting mobile/web.

## Important Packages And Frameworks

| Area | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| UI | uGUI, TextMesh Pro | Confirmed | `Packages/manifest.json`; scene components |
| Rendering | URP 17.3.0 | Confirmed | package manifest and `ProjectSettings/GraphicsSettings.asset` |
| Speech recognition | Whisper C++ native wrapper via P/Invoke; base/small/medium English model assets | Confirmed | `Assets/Scripts/Whisper/WhisperNative.cs`, `Native/WhisperUnityWrapper/`, `Assets/StreamingAssets/Whisper/` |
| Answer evaluation | Local Flask service sends prompts to llama.cpp server; Unity client posts to port 5000 | Confirmed | `local-llm/local_evaluator.py`, `Assets/Scripts/RTR/LocalAnswerEvaluator.cs`, `Assets/Editor/RTRLocalAIServerWindow.cs` |
| Algorithmic matching | RapidFuzz `fuzz.ratio` runs in the local Flask service; Unity posts expected/student answers to `/algorithmic_evaluate` | Confirmed | `local-llm/local_evaluator.py`, `local-llm/requirements.txt`, `RTRAnswerEvaluatorTester.cs` |
| Speech synthesis | Local Kokoro ONNX server; Unity client posts to port 5001 | Confirmed | `local-tts/server.py`, `Assets/Scripts/TTS/RTRLocalTTS.cs` |
| ONNX Runtime | Asus4 Unity ONNX Runtime package 0.4.9 | Confirmed | `Packages/manifest.json`; used by Kokoro test script |
| Tests | Unity Test Framework 1.6.0 dependency; no first-party test assemblies or authored tests found | Confirmed | package manifest; no `.asmdef` or test files |
| Networking/multiplayer | No first-party multiplayer usage found; package `com.unity.multiplayer.center` is present | Confirmed package; no app usage found | manifest and first-party source search |

## Directory Structure

| Path | Purpose | Confidence | Evidence |
| --- | --- | --- | --- |
| `Assets/Scripts/` | Runtime question navigation, UI, answer evaluation, timers, Whisper recording, and TTS clients | Confirmed | source inspection |
| `Assets/ScriptableObject/` | Reusable data definitions and scenario content organized by aircraft/flight phase | Confirmed | source inspection and asset folders |
| `Assets/Scenes/` | Main simulator scene | Confirmed | build settings |
| `Assets/Prefabs/` | Reusable interaction/UI elements | Confirmed | asset inventory |
| `Assets/Audio/`, `Assets/Images/` | Scenario audio, maps, and icons | Confirmed | asset inventory |
| `Assets/StreamingAssets/Whisper/` | Whisper model binaries loaded by native code | Confirmed | recorder and file inventory |
| `Assets/Editor/` | Editor-only utility for starting local LLM and evaluator processes | Confirmed | `RTRLocalAIServerWindow.cs` |
| `Assets/Settings/` | URP and rendering settings | Confirmed | asset inventory |
| `Native/WhisperUnityWrapper/` | CMake/C++ P/Invoke bridge to Whisper | Confirmed | native source inventory |
| `local-llm/` | llama.cpp checkout/model and Python evaluation service | Confirmed | editor launcher and Python source |
| `local-tts/` | Python Kokoro TTS server, model/voice files, and local virtual environment | Confirmed | service source and inventory |
| `whisper.cpp/` | Whisper.cpp source checkout currently untracked in working tree | Confirmed | git status and directory inventory |

## Assembly Boundaries

- No `.asmdef` or `.asmref` files found. First-party C# uses Unity's default `Assembly-CSharp` and `Assembly-CSharp-Editor` assemblies.
- Runtime and editor code are separated by the conventional `Assets/Editor/` folder, but runtime code remains one broad assembly.

## Scenes And Startup Flow

- Enabled build scene: `Assets/Scenes/RTR_SIM.unity` only (`ProjectSettings/EditorBuildSettings.asset`).
- Likely startup: `RTR_SIM`; no separate bootstrap or scene transition flow was found in the inspected code.
- Scene contains the dataset/UI controller, question navigator, interaction manager, answer evaluation manager, microphone/Whisper, TTS, timer, plus ONNX/Whisper test objects. Exact active state and serialized references should be verified in the Unity Editor; no Editor/MCP connection was available.

## Architecture

| Pattern | Finding | Confidence | Evidence |
| --- | --- | --- | --- |
| Runtime structure | MonoBehaviour-driven uGUI scene; components communicate through serialized references and button listeners | Confirmed | scene and scripts |
| Scenario data | ScriptableObject assets model datasets, flight phases, questions, aircraft/frequency/METAR information, and interactions | Confirmed | `RTRDataSet.cs`, `FlightPhaseData.cs`, `QuestionData.cs` |
| Main flow | `RTRQuestionNavigator` walks questions/phases and interactions, updates the data/UI controller, and asks interaction controller to display/play ATC prompts | Confirmed | `RTRQuestionNavigator.cs` |
| Evaluation | `IAnswerEvaluator` abstraction with local HTTP evaluator and tester implementations | Confirmed | `IAnswerEvaluator.cs`, `LocalAnswerEvaluator.cs`, `RTRAnswerEvaluatorTester.cs` |
| Algorithmic evaluator | Normalizes common `2000`/`8000` numeric phrases and punctuation, returns RapidFuzz similarity (integer 0–100), considers >=85 correct, returns empty severity, and includes the precise ratio in `reason` | Confirmed | `local-llm/local_evaluator.py`, `RTRAnswerEvaluatorTester.cs` |
| Speech recognition | Microphone capture -> native Whisper P/Invoke -> transcript UI/input | Confirmed | `WhisperRealtimeRecorder.cs`, `WhisperNative.cs` |
| Local AI lifecycle | Editor window can start llama.cpp plus the evaluator for local LLM mode, or start only the evaluator for RapidFuzz mode; TTS server lifecycle is not handled there | Confirmed | `RTRLocalAIServerWindow.cs`, `local-tts/server.py` |
| Persistence | No user-result persistence/save flow identified in inspected first-party scripts | Unknown beyond inspected scope | script inventory |

## Coding Conventions

- No namespaces in inspected first-party scripts; public MonoBehaviour/data classes largely map to file names.
- Inspector wiring uses `[SerializeField] private` fields; public fields are used for serializable ScriptableObject data models.
- Async work uses `Task`/`async` for HTTP evaluator and background Whisper transcription; UnityWebRequest operations are polled.
- Comments are verbose section banners; naming is PascalCase for types/methods and camelCase for fields.
- No project-level formatter or analyzer config was found in the root inventory.

## Testing And Validation

- Unity Test Framework package is installed; no project-authored EditMode/PlayMode tests or test assemblies found.
- No CI workflow or documented build/test command was found in root guidance files.
- Python service test helper: `local-tts/test_tts.py`; not run as part of onboarding.

## Available Unity Tooling

- Unity-specific MCP/editor tools: unavailable in this session. Repository inspection only; no scenes were opened or modified and no tests/build were run.

## Important Constraints

- Runtime evaluation expects a local evaluator at `127.0.0.1:5000`; TTS expects `127.0.0.1:5001`; the editor launcher manages LLM at `127.0.0.1:8080` and evaluator at port 5000.
- The algorithmic evaluator also depends on RapidFuzz installed from `local-llm/requirements.txt` into `local-llm/python-packages/`; start it with **DGCA RTR/Local AI Server → START RAPIDFUZZ ONLY**. The 85% cutoff is heuristic and does not check aviation-critical tokens separately.
- Whisper runtime requires the native `WhisperUnityWrapper` library and model binaries. Native build/import configuration and target-platform support need verification.
- The local TTS server requires its Python environment, model, and voice assets; the editor local-AI launcher does not start it.
- Do not overwrite the user's working-tree edits. At analysis time modified files included `.gitignore`, the main scene, `RTRAnswerEvaluatorTester.cs`, `RTRQuestionNavigator.cs`, and `WhisperRealtimeRecorder.cs`; untracked files included a question-indicator prefab/status tracker and `whisper.cpp/`.

## Unknowns And Confidence

- Runtime behavior, object activation, and exact serialized wiring in the main scene are only partially mapped from YAML; confirm in Editor before changing scene logic.
- Supported build platforms and packaging/distribution workflow are unknown.
- Whether the question evaluator's external hosted fallback remains intended is unclear.
- No Unity Editor connection was available to inspect Console state, active scene hierarchy, or build settings interactively.

## Source Files Inspected

- `ProjectSettings/ProjectVersion.txt`, `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/GraphicsSettings.asset`, `ProjectSettings/ProjectSettings.asset`
- `Packages/manifest.json`
- `Assets/Scripts/RTRQuestionNavigator.cs`, `Assets/Scripts/RTR/LocalAnswerEvaluator.cs`, `Assets/Scripts/RTR/IAnswerEvaluator.cs`, `Assets/Scripts/RTR/RTRValidationResult.cs`, `Assets/Scripts/Whisper/WhisperNative.cs`, `Assets/Scripts/Whisper/WhisperRealtimeRecorder.cs`, `Assets/Scripts/TTS/RTRLocalTTS.cs`
- `Assets/ScriptableObject/RTRDataSet/RTRDataSet.cs`, `Assets/ScriptableObject/Phase/FlightPhaseData.cs`, `Assets/ScriptableObject/Questions/QuestionData.cs`
- `Assets/Editor/RTRLocalAIServerWindow.cs`, `local-llm/local_evaluator.py`, `local-tts/server.py`
- `Assets/Scenes/RTR_SIM.unity` (serialized object names/components), `Native/WhisperUnityWrapper/`, repository status and directory inventory

<!-- unity-onboarding:generated:end -->
