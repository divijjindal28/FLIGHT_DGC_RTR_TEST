from flask import Flask, request, send_file, jsonify
from kokoro_onnx import Kokoro
import soundfile as sf
import tempfile
import os
import time
import uuid
import threading

app = Flask(__name__)

MODEL_PATH = "models/kokoro-v1.0.fp16.onnx"
VOICES_PATH = "voices/voices-v1.0.bin"

VOICE = "af_bella"
SPEED = 1.0
LANG = "en-us"

print("Loading Kokoro...")

kokoro = Kokoro(
    MODEL_PATH,
    VOICES_PATH
)

print()
print("========================================")
print("KOKORO RUNTIME INFORMATION")

try:
    print(
        "Kokoro object:",
        type(kokoro)
    )

    print(
        "Kokoro attributes:",
        dir(kokoro)
    )

except Exception as exception:

    print(
        "Could not inspect Kokoro:",
        exception
    )

print("========================================")

print("Kokoro loaded successfully.")


@app.route("/tts", methods=["POST"])
def text_to_speech():

    request_start = time.perf_counter()

    request_id = request.headers.get(
        "X-TTS-Request-ID",
        str(uuid.uuid4())
    )

    print()
    print("========================================")
    print("TTS REQUEST RECEIVED")
    print("Request ID:", request_id)
    print("Thread ID:", threading.get_ident())
    print("Time:", time.strftime("%H:%M:%S"))

    # --------------------------------------------------
    # RAW REQUEST INFORMATION
    # --------------------------------------------------

    raw_body = request.get_data()

    print(
        "Raw body bytes:",
        len(raw_body)
    )

    print(
        "Raw body:",
        raw_body.decode("utf-8")
    )

    print(
        "Content-Type:",
        request.headers.get("Content-Type")
    )

    # --------------------------------------------------
    # JSON PARSING
    # --------------------------------------------------

    json_start = time.perf_counter()

    data = request.get_json()

    json_time = (
        time.perf_counter() -
        json_start
    )

    print(
        "JSON parsing time:",
        round(json_time, 6),
        "seconds"
    )

    if not data or "text" not in data:

        return jsonify({
            "error": "Missing 'text' field."
        }), 400

    text = data["text"].strip()

    if not text:

        return jsonify({
            "error": "Text is empty."
        }), 400

    # --------------------------------------------------
    # EXACT KOKORO PARAMETERS
    # --------------------------------------------------

    voice = VOICE
    speed = SPEED
    lang = LANG

    print()
    print("KOKORO PARAMETERS")
    print("Text:", repr(text))
    print("Text length:", len(text))
    print("Voice:", voice)
    print("Speed:", speed)
    print("Language:", lang)
    print("Model:", MODEL_PATH)
    print("Voices:", VOICES_PATH)

    # --------------------------------------------------
    # KOKORO GENERATION
    # --------------------------------------------------

    generation_start = time.perf_counter()

    print()
    print("Starting Kokoro generation...")

    try:

        samples, sample_rate = kokoro.create(
            text,
            voice=voice,
            speed=speed,
            lang=lang
        )

        generation_time = (
            time.perf_counter() -
            generation_start
        )

        print(
            "Kokoro generation completed in:",
            round(generation_time, 6),
            "seconds"
        )

        print(
            "Sample rate:",
            sample_rate
        )

        print(
            "Sample count:",
            len(samples)
        )

        # --------------------------------------------------
        # WAV WRITING
        # --------------------------------------------------

        wav_start = time.perf_counter()

        temp_file = tempfile.NamedTemporaryFile(
            suffix=".wav",
            delete=False
        )

        temp_file.close()

        sf.write(
            temp_file.name,
            samples,
            sample_rate
        )

        wav_time = (
            time.perf_counter() -
            wav_start
        )

        print(
            "WAV writing completed in:",
            round(wav_time, 6),
            "seconds"
        )

        # --------------------------------------------------
        # TOTAL SERVER TIME BEFORE RESPONSE
        # --------------------------------------------------

        total_server_time = (
            time.perf_counter() -
            request_start
        )

        print()
        print(
            "TOTAL SERVER PROCESSING TIME:",
            round(total_server_time, 6),
            "seconds"
        )

        print("Response created.")
        print("Request ID:", request_id)
        print("========================================")

        response = send_file(
            temp_file.name,
            mimetype="audio/wav",
            as_attachment=False
        )

        response.headers["X-TTS-Request-ID"] = request_id
        response.headers["X-TTS-Generation-Time"] = str(
            generation_time
        )

        @response.call_on_close
        def cleanup():

            try:
                os.remove(temp_file.name)

            except OSError:
                pass

        return response

    except Exception as exception:

        print()
        print("TTS ERROR:")
        print(exception)
        print("========================================")

        return jsonify({
            "error": str(exception)
        }), 500


@app.route("/health", methods=["GET"])
def health():

    return jsonify({
        "status": "ok",
        "voice": VOICE,
        "speed": SPEED,
        "language": LANG
    })


if __name__ == "__main__":

    print()
    print("Starting local TTS server...")
    print("Voice:", VOICE)
    print("Speed:", SPEED)
    print("Language:", LANG)
    print("Address: http://127.0.0.1:5001")

    app.run(
        host="127.0.0.1",
        port=5001,
        debug=False
    )