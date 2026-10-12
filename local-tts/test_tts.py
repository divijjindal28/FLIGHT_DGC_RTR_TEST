from kokoro_onnx import Kokoro
import soundfile as sf
import time

MODEL_PATH = "models/kokoro-v1.0.fp16.onnx"
VOICES_PATH = "voices/voices-v1.0.bin"

TEXT = "S E I 9 3 3 , pushback and start-up approved.."

VOICE = "af_bella"
print("Loading Kokoro...")

kokoro = Kokoro(
    MODEL_PATH,
    VOICES_PATH
)

print("Kokoro loaded.")

start_time = time.time()

samples, sample_rate = kokoro.create(
    TEXT,
    voice=VOICE,
    speed=0.9,
    lang="en-us"
)

generation_time = time.time() - start_time

output_file = "test_output.wav"

sf.write(
    output_file,
    samples,
    sample_rate
)

print()
print("TTS generation complete.")
print("Generation time:", round(generation_time, 2), "seconds")
print("Sample rate:", sample_rate)
print("Output:", output_file)