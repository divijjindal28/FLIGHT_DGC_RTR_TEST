using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class RTRLocalAIServerWindow : EditorWindow
{
    // --------------------------------------------------
    // Processes
    // --------------------------------------------------

    private static Process llamaProcess;
    private static Process evaluatorProcess;


    // --------------------------------------------------
    // Paths
    // --------------------------------------------------

    private string llamaServerPath;
    private string modelPath;

    private string pythonPath =
        @"C:\Users\divij\AppData\Local\Programs\Python\Python311\python.exe";

    private string evaluatorScriptPath;


    // --------------------------------------------------
    // Status
    // --------------------------------------------------

    private string statusMessage =
        "LOCAL RTR AI STOPPED";

    private bool isStarting = false;

    private UnityWebRequest readinessRequest;

    private enum StartupStage
    {
        None,
        WaitingForLLM,
        StartingEvaluator,
        WaitingForEvaluator,
        Ready
    }

    private StartupStage startupStage =
        StartupStage.None;


    // --------------------------------------------------
    // Menu
    // --------------------------------------------------

    [MenuItem("DGCA RTR/Local AI Server")]
    public static void ShowWindow()
    {
        GetWindow<RTRLocalAIServerWindow>(
            "DGCA RTR Local AI"
        );
    }


    // --------------------------------------------------
    // Enable
    // --------------------------------------------------

    private void OnEnable()
    {
        string projectRoot =
            Directory.GetParent(
                Application.dataPath
            ).FullName;

        llamaServerPath =
            Path.Combine(
                projectRoot,
                "local-llm",
                "llama.cpp",
                "llama-server.exe"
            );

        modelPath =
            Path.Combine(
                projectRoot,
                "local-llm",
                "models",
                "Qwen3-8B-Q4_K_M.gguf"
            );

        evaluatorScriptPath =
            Path.Combine(
                projectRoot,
                "local-llm",
                "local_evaluator.py"
            );

        Repaint();
    }


    // --------------------------------------------------
    // GUI
    // --------------------------------------------------

    private void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.LabelField(
            "DGCA RTR Local AI",
            EditorStyles.boldLabel
        );

        GUILayout.Space(10);


        // --------------------------------------------------
        // LLM Server
        // --------------------------------------------------

        EditorGUILayout.LabelField(
            "LLM Server",
            EditorStyles.boldLabel
        );

        llamaServerPath =
            EditorGUILayout.TextField(
                "llama-server.exe",
                llamaServerPath
            );

        modelPath =
            EditorGUILayout.TextField(
                "Model",
                modelPath
            );

        GUILayout.Space(5);

        if (IsLLMRunning())
        {
            EditorGUILayout.HelpBox(
                "LLM Server: RUNNING",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "LLM Server: STOPPED",
                MessageType.Warning
            );
        }


        GUILayout.Space(10);


        // --------------------------------------------------
        // RTR Evaluator
        // --------------------------------------------------

        EditorGUILayout.LabelField(
            "RTR Evaluator",
            EditorStyles.boldLabel
        );

        pythonPath =
            EditorGUILayout.TextField(
                "Python",
                pythonPath
            );

        evaluatorScriptPath =
            EditorGUILayout.TextField(
                "Evaluator Script",
                evaluatorScriptPath
            );

        GUILayout.Space(5);

        if (IsEvaluatorRunning())
        {
            EditorGUILayout.HelpBox(
                "RTR Evaluator: RUNNING",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(
                "RTR Evaluator: STOPPED",
                MessageType.Warning
            );
        }


        GUILayout.Space(15);


        // --------------------------------------------------
        // Overall Status
        // --------------------------------------------------

        MessageType statusType =
            MessageType.Info;

        if (startupStage ==
            StartupStage.WaitingForLLM ||
            startupStage ==
            StartupStage.StartingEvaluator ||
            startupStage ==
            StartupStage.WaitingForEvaluator)
        {
            statusType =
                MessageType.Warning;
        }

        if (startupStage == StartupStage.None)
        {
            statusType =
                MessageType.Warning;
        }

        EditorGUILayout.HelpBox(
            statusMessage,
            statusType
        );


        GUILayout.Space(10);


        // --------------------------------------------------
        // Start
        // --------------------------------------------------

        GUI.enabled =
            !isStarting &&
            (!IsLLMRunning() ||
             !IsEvaluatorRunning());

        if (GUILayout.Button(
            "START LOCAL RTR AI",
            GUILayout.Height(40)))
        {
            StartLocalAI();
        }


        GUI.enabled = true;

        GUILayout.Space(5);


        // --------------------------------------------------
        GUI.enabled = true;

        GUILayout.Space(5);

        GUI.enabled = !isStarting && !IsEvaluatorRunning();

        if (GUILayout.Button(
            "START RAPIDFUZZ ONLY",
            GUILayout.Height(35)))
        {
            StartAlgorithmicEvaluator();
        }

        GUI.enabled = true;
        GUILayout.Space(5);
        // Stop
        // --------------------------------------------------

        GUI.enabled =
            !isStarting &&
            (IsLLMRunning() ||
             IsEvaluatorRunning());

        if (GUILayout.Button(
            "STOP LOCAL RTR AI",
            GUILayout.Height(35)))
        {
            StopLocalAI();
        }

        GUI.enabled = true;


        GUILayout.Space(15);


        // --------------------------------------------------
        // Services
        // --------------------------------------------------

        EditorGUILayout.LabelField(
            "Services",
            EditorStyles.boldLabel
        );

        EditorGUILayout.LabelField(
            "LLM Server:",
            "http://127.0.0.1:8080"
        );

        EditorGUILayout.LabelField(
            "RTR Evaluator:",
            "http://127.0.0.1:5000"
        );
    }


    // --------------------------------------------------
    // Start Local AI
    // --------------------------------------------------

    // --------------------------------------------------
    // Start Algorithmic Evaluator Without LLM
    // --------------------------------------------------

    private void StartAlgorithmicEvaluator()
    {
        if (isStarting || IsEvaluatorRunning())
            return;

        if (!File.Exists(pythonPath))
        {
            statusMessage = "ERROR: Python executable not found.";
            Repaint();
            return;
        }

        if (!File.Exists(evaluatorScriptPath))
        {
            statusMessage = "ERROR: local_evaluator.py not found.";
            Repaint();
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string rapidFuzzPath = Path.Combine(
            projectRoot,
            "local-llm",
            "python-packages",
            "rapidfuzz",
            "__init__.py"
        );

        if (!File.Exists(rapidFuzzPath))
        {
            statusMessage = "ERROR: Install local-llm requirements first.";
            Repaint();
            return;
        }

        isStarting = true;
        startupStage = StartupStage.StartingEvaluator;
        statusMessage = "STARTING RAPIDFUZZ EVALUATOR...";

        StartEvaluator();

        if (!IsEvaluatorRunning())
        {
            StartupFailed("Could not start RapidFuzz evaluator.");
            return;
        }

        startupStage = StartupStage.WaitingForEvaluator;
        WaitForService(
            "http://127.0.0.1:5000/health",
            OnAlgorithmicEvaluatorReady
        );
    }


    private void OnAlgorithmicEvaluatorReady()
    {
        startupStage = StartupStage.Ready;
        isStarting = false;
        statusMessage = "RAPIDFUZZ EVALUATOR READY (LLM NOT STARTED)";
        Repaint();
    }

    private void StartLocalAI()
    {
        if (isStarting)
            return;


        // --------------------------------------------------
        // Validate files
        // --------------------------------------------------

        if (!File.Exists(llamaServerPath))
        {
            UnityEngine.Debug.LogError(
                "llama-server.exe not found:\n" +
                llamaServerPath
            );

            statusMessage =
                "ERROR: llama-server.exe not found.";

            Repaint();

            return;
        }


        if (!File.Exists(modelPath))
        {
            UnityEngine.Debug.LogError(
                "Qwen model not found:\n" +
                modelPath
            );

            statusMessage =
                "ERROR: Qwen model not found.";

            Repaint();

            return;
        }


        if (!File.Exists(pythonPath))
        {
            UnityEngine.Debug.LogError(
                "Python executable not found:\n" +
                pythonPath
            );

            statusMessage =
                "ERROR: Python executable not found.";

            Repaint();

            return;
        }


        if (!File.Exists(evaluatorScriptPath))
        {
            UnityEngine.Debug.LogError(
                "local_evaluator.py not found:\n" +
                evaluatorScriptPath
            );

            statusMessage =
                "ERROR: local_evaluator.py not found.";

            Repaint();

            return;
        }


        isStarting = true;


        // --------------------------------------------------
        // Start LLM if necessary
        // --------------------------------------------------

        if (!IsLLMRunning())
        {
            StartLLMServer();
        }


        // --------------------------------------------------
        // Check LLM readiness
        // --------------------------------------------------

        if (IsLLMRunning())
        {
            startupStage =
                StartupStage.WaitingForLLM;

            statusMessage =
                "WAITING FOR LLM SERVER...";

            WaitForService(
                "http://127.0.0.1:8080/",
                OnLLMReady
            );
        }
        else
        {
            StartupFailed(
                "Could not start llama-server."
            );
        }


        Repaint();
    }


    // --------------------------------------------------
    // Start LLM
    // --------------------------------------------------

    private void StartLLMServer()
    {
        if (IsLLMRunning())
            return;


        string llamaDirectory =
            Path.GetDirectoryName(
                llamaServerPath
            );


        ProcessStartInfo startInfo =
            new ProcessStartInfo();

        startInfo.FileName =
            llamaServerPath;

        startInfo.WorkingDirectory =
            llamaDirectory;

        startInfo.Arguments =
            "-m \"" +
            modelPath +
            "\" " +
            "-ngl 99 " +
            "-c 4096";


        // Keep the terminal visible during development.
        startInfo.UseShellExecute =
            true;

        startInfo.CreateNoWindow =
            false;


        try
        {
            llamaProcess =
                Process.Start(
                    startInfo
                );

            UnityEngine.Debug.Log(
                "Started llama-server."
            );
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                "Could not start llama-server:\n" +
                exception.Message
            );

            llamaProcess = null;
        }
    }


    // --------------------------------------------------
    // LLM Ready
    // --------------------------------------------------

    private void OnLLMReady()
    {
        UnityEngine.Debug.Log(
            "LLM server is ready."
        );


        startupStage =
            StartupStage.StartingEvaluator;

        statusMessage =
            "LLM READY — STARTING RTR EVALUATOR...";


        Repaint();


        // --------------------------------------------------
        // Start Python evaluator
        // --------------------------------------------------

        if (!IsEvaluatorRunning())
        {
            StartEvaluator();
        }


        if (IsEvaluatorRunning())
        {
            startupStage =
                StartupStage.WaitingForEvaluator;

            statusMessage =
                "WAITING FOR RTR EVALUATOR...";


            WaitForService(
                "http://127.0.0.1:5000/evaluate",
                OnEvaluatorReady
            );
        }
        else
        {
            StartupFailed(
                "Could not start RTR evaluator."
            );
        }
    }


    // --------------------------------------------------
    // Start Python Evaluator
    // --------------------------------------------------

    private void StartEvaluator()
    {
        if (IsEvaluatorRunning())
            return;


        string evaluatorDirectory =
            Path.GetDirectoryName(
                evaluatorScriptPath
            );


        ProcessStartInfo startInfo =
            new ProcessStartInfo();

        startInfo.FileName =
            pythonPath;

        startInfo.WorkingDirectory =
            evaluatorDirectory;

        startInfo.Arguments =
            "\"" +
            evaluatorScriptPath +
            "\"";


        // Keep terminal visible during development.
        startInfo.UseShellExecute =
            true;

        startInfo.CreateNoWindow =
            false;


        try
        {
            evaluatorProcess =
                Process.Start(
                    startInfo
                );

            UnityEngine.Debug.Log(
                "Started local_evaluator.py."
            );
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                "Could not start local_evaluator.py:\n" +
                exception.Message
            );

            evaluatorProcess = null;
        }
    }


    // --------------------------------------------------
    // Evaluator Ready
    // --------------------------------------------------

    private void OnEvaluatorReady()
    {
        startupStage =
            StartupStage.Ready;

        isStarting = false;

        statusMessage =
            "LOCAL RTR AI READY";

        UnityEngine.Debug.Log(
            "DGCA RTR Local AI is READY."
        );

        Repaint();
    }


    // --------------------------------------------------
    // Wait For Service
    // --------------------------------------------------

    private void WaitForService(
        string url,
        Action onReady
    )
    {
        if (readinessRequest != null)
        {
            readinessRequest.Dispose();
            readinessRequest = null;
        }


        readinessRequest =
            UnityWebRequest.Get(url);

        readinessRequest.timeout = 1;


        readinessRequest.SendWebRequest();


        EditorApplication.update -=
            CheckServiceRequest;

        EditorApplication.update +=
            CheckServiceRequest;


        void CheckServiceRequest()
        {
            if (readinessRequest == null)
            {
                EditorApplication.update -=
                    CheckServiceRequest;

                return;
            }


            if (!readinessRequest.isDone)
                return;


            EditorApplication.update -=
                CheckServiceRequest;


            long responseCode =
                readinessRequest.responseCode;


            bool serviceResponded =
                responseCode > 0;


            readinessRequest.Dispose();
            readinessRequest = null;


            if (serviceResponded)
            {
                onReady();
                return;
            }


            // --------------------------------------------------
            // Service is not ready yet.
            // Check again.
            // --------------------------------------------------

            EditorApplication.delayCall +=
                () =>
                {
                    if (isStarting)
                    {
                        WaitForService(
                            url,
                            onReady
                        );
                    }
                };
        }
    }


    // --------------------------------------------------
    // Stop Local AI
    // --------------------------------------------------

    private void StopLocalAI()
    {
        isStarting = false;

        startupStage =
            StartupStage.None;


        if (readinessRequest != null)
        {
            readinessRequest.Dispose();
            readinessRequest = null;
        }


        StopProcess(
            ref evaluatorProcess,
            "RTR Evaluator"
        );


        StopProcess(
            ref llamaProcess,
            "LLM Server"
        );


        statusMessage =
            "LOCAL RTR AI STOPPED";


        Repaint();


        UnityEngine.Debug.Log(
            "DGCA RTR Local AI stopped."
        );
    }


    // --------------------------------------------------
    // Stop Process
    // --------------------------------------------------

    private void StopProcess(
        ref Process process,
        string processName
    )
    {
        if (process == null)
            return;


        try
        {
            if (!process.HasExited)
            {
                process.Kill();

                process.WaitForExit(
                    2000
                );
            }
        }
        catch (Exception exception)
        {
            UnityEngine.Debug.LogError(
                "Could not stop " +
                processName +
                ":\n" +
                exception.Message
            );
        }
        finally
        {
            process.Dispose();
            process = null;
        }
    }


    // --------------------------------------------------
    // Status
    // --------------------------------------------------

    private bool IsLLMRunning()
    {
        try
        {
            return llamaProcess != null &&
                   !llamaProcess.HasExited;
        }
        catch
        {
            return false;
        }
    }


    private bool IsEvaluatorRunning()
    {
        try
        {
            return evaluatorProcess != null &&
                   !evaluatorProcess.HasExited;
        }
        catch
        {
            return false;
        }
    }


    // --------------------------------------------------
    // Startup Failed
    // --------------------------------------------------

    private void StartupFailed(
        string message
    )
    {
        isStarting = false;

        startupStage =
            StartupStage.None;

        statusMessage =
            "ERROR: " + message;

        UnityEngine.Debug.LogError(
            message
        );

        Repaint();
    }


    // --------------------------------------------------
    // Cleanup
    // --------------------------------------------------

    private void OnDisable()
    {
        // Intentionally do not stop the servers
        // when this Editor window is closed.
    }
}