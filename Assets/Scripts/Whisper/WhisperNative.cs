using System;
using System.Runtime.InteropServices;

public static class WhisperNative
{
    private const string DLL_NAME = "WhisperUnityWrapper";

    [DllImport(
        DLL_NAME,
        CallingConvention = CallingConvention.Cdecl
    )]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool Whisper_LoadModel(
        [MarshalAs(UnmanagedType.LPStr)] string modelPath
    );

    [DllImport(
        DLL_NAME,
        CallingConvention = CallingConvention.Cdecl
    )]
    public static extern void Whisper_UnloadModel();

    [DllImport(
        DLL_NAME,
        CallingConvention = CallingConvention.Cdecl
    )]
    private static extern IntPtr Whisper_Transcribe(
        float[] audioData,
        int sampleCount
    );

    public static string Transcribe(float[] audioData)
    {
        if (audioData == null || audioData.Length == 0)
            return string.Empty;

        IntPtr result = Whisper_Transcribe(
            audioData,
            audioData.Length
        );

        if (result == IntPtr.Zero)
            return string.Empty;

        return Marshal.PtrToStringAnsi(result);
    }
}