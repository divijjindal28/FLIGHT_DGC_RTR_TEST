#include "whisper.h"

#include <string>
#include <vector>
#include <mutex>

static whisper_context* g_context = nullptr;
static std::mutex g_mutex;

extern "C"
{

__declspec(dllexport)
bool Whisper_LoadModel(const char* modelPath)
{
    std::lock_guard<std::mutex> lock(g_mutex);

    if (g_context != nullptr)
    {
        whisper_free(g_context);
        g_context = nullptr;
    }

    if (modelPath == nullptr)
        return false;

    whisper_context_params cparams = whisper_context_default_params();

    g_context = whisper_init_from_file_with_params(
        modelPath,
        cparams
    );

    return g_context != nullptr;
}


__declspec(dllexport)
void Whisper_UnloadModel()
{
    std::lock_guard<std::mutex> lock(g_mutex);

    if (g_context != nullptr)
    {
        whisper_free(g_context);
        g_context = nullptr;
    }
}


__declspec(dllexport)
const char* Whisper_Transcribe(
    const float* audioData,
    int sampleCount
)
{
    static std::string result;

    std::lock_guard<std::mutex> lock(g_mutex);

    result.clear();

    if (g_context == nullptr)
        return nullptr;

    if (audioData == nullptr || sampleCount <= 0)
        return nullptr;

    whisper_full_params params =
        whisper_full_default_params(WHISPER_SAMPLING_GREEDY);

    params.print_progress   = false;
    params.print_special    = false;
    params.print_realtime   = false;
    params.print_timestamps = false;

    params.language = "en";
    params.translate = false;

    int resultCode = whisper_full(
        g_context,
        params,
        audioData,
        sampleCount
    );

    if (resultCode != 0)
        return nullptr;

    int segmentCount = whisper_full_n_segments(g_context);

    for (int i = 0; i < segmentCount; ++i)
    {
        const char* text =
            whisper_full_get_segment_text(g_context, i);

        if (text != nullptr)
            result += text;
    }

    return result.c_str();
}

}