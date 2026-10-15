import json
import os
import re
import sys
import requests
from flask import Flask, request, jsonify

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "python-packages"))

from rapidfuzz import fuzz


# --------------------------------------------------
# Local LLM
# --------------------------------------------------

LLAMA_SERVER_URL = "http://127.0.0.1:8080/v1/chat/completions"


# --------------------------------------------------
# RTR Evaluation Rules
# --------------------------------------------------

RTR_SYSTEM_PROMPT = """
You are an aviation radiotelephony assessment evaluator
for a DGCA RTR Part 2 training system.

Your task is to evaluate a student's spoken radiotelephony
response against the supplied question, scenario description,
and expected answer.

The expected answer is the reference answer provided by the
training system. Do not replace it with an answer from your
own knowledge.

EVALUATION PRINCIPLES

1. Evaluate the student's operational meaning, not exact
   word-for-word similarity.

2. Do not require the student to reproduce the expected
   answer exactly.

3. Accept equivalent wording and equivalent phraseology
   when the operational meaning is unchanged.

4. Accept harmless differences in:
   - grammar
   - word order
   - punctuation
   - capitalization
   - natural spoken phrasing

5. The student's answer may come from speech-to-text.
   Do not penalize obvious minor transcription errors when
   the intended meaning is still clear.

6. Pay particular attention to operationally important
   information, including:
   - aircraft callsign
   - runway
   - altitude
   - flight level
   - heading
   - direction
   - frequency
   - position
   - clearance
   - traffic information
   - emergency or distress status
   - requested action
   - destination
   - diversion
   - other information that changes the operational meaning

7. If the student gives an operationally important value
   that conflicts with the expected answer, treat this as
   a significant error.

8. If the student fails to communicate an important action
   or piece of information required by the scenario and
   expected answer, identify it as missing.

9. Do not invent additional requirements merely because they
   are common in aviation. Evaluate the student against the
   supplied question, description, and expected answer.

10. A response can use different wording from the expected
    answer and still be fully correct if the operational
    meaning is equivalent.

11. If the student's response is ambiguous because of
    speech-to-text errors, use the most reasonable
    interpretation of the student's intended meaning.
    Only penalize it when the ambiguity affects operational
    correctness.

12. Do not judge the answer based on superficial text
    similarity.

SCORING

Use a score from 0 to 100.

100:
Operationally correct. The response communicates the required
meaning and important information. Only harmless wording or
transcription differences may exist.

90-99:
Essentially correct, with a minor non-operational issue that
does not affect the operational meaning.

70-89:
Partially correct. One meaningful element is missing, unclear,
or insufficiently communicated, but the main scenario is
understood.

40-69:
Major error. Important operational information or action is
missing or incorrect.

0-39:
Substantially incorrect, inappropriate, unrelated, or does
not adequately respond to the scenario.

CORRECTNESS

Set correctness to true only when the student's response is
operationally acceptable for the supplied scenario.

A response may have a score below 100 and still be acceptable
if the issue is only minor and non-operational.

SEVERITY

Use one of:

"none":
The response is fully acceptable.

"minor":
There is a minor issue that does not materially affect
operational meaning.

"major":
There is a meaningful operational error or omission.

"critical":
There is a serious safety-related, emergency-related,
clearance-related, or otherwise highly significant
operational error.

OUTPUT FORMAT

Return ONLY valid JSON using exactly this structure:

{
  "score": 0,
  "correctness": false,
  "severity": "none",
  "reason": ""
}

Rules for the fields:

score:
An integer from 0 to 100.

correctness:
true if the student's response is operationally acceptable.
false otherwise.

severity:
Must be exactly one of:
"none", "minor", "major", "critical"

reason:
A concise explanation of why the student's response received
this score.

Do not add any other fields.
Do not use Markdown.
Do not use ```json fences.
"""


# --------------------------------------------------
# Flask
# --------------------------------------------------

app = Flask(__name__)


# --------------------------------------------------
# Evaluation
# --------------------------------------------------

@app.route("/evaluate", methods=["POST"])
def evaluate():

    data = request.get_json()

    if not data:
        return jsonify({
            "error": "No JSON data received."
        }), 400

    question = data.get("question", "")
    description = data.get("description", "")
    expected_answer = data.get("expected_answer", "")
    student_answer = data.get("student_answer", "")

    # --------------------------------------------------
    # Build evaluation prompt
    # --------------------------------------------------

    prompt = f"""
Evaluate the student's response using the evaluation rules.

QUESTION:
{question}

SCENARIO DESCRIPTION:
{description}

EXPECTED ANSWER:
{expected_answer}

STUDENT ANSWER:
{student_answer}
"""

    # --------------------------------------------------
    # Send to local llama-server
    # --------------------------------------------------

    payload = {
    "messages": [
        {"role": "system", "content": RTR_SYSTEM_PROMPT},
        {"role": "user", "content": prompt}
    ],
    "temperature": 0.1,
    "max_tokens": 1000
}

    try:

        response = requests.post(
            LLAMA_SERVER_URL,
            json=payload,
            timeout=120
        )

        response.raise_for_status()

        result = response.json()

    except Exception as exception:

        return jsonify({
            "error": str(exception)
        }), 500

    # --------------------------------------------------
    # Extract model response
    # --------------------------------------------------

    try:

        content = (
            result["choices"][0]
            ["message"]
            ["content"]
        )

    except Exception:

        return jsonify({
            "error": "Could not extract model response.",
            "raw_response": result
        }), 500

    if content is None:

        return jsonify({
            "error": "Local LLM returned no content.",
            "raw_response": result
        }), 500

    # --------------------------------------------------
    # Clean Markdown fences if model adds them
    # --------------------------------------------------

    content = content.strip()

    if content.startswith("```json"):
        content = content[7:]

    if content.startswith("```"):
        content = content[3:]

    if content.endswith("```"):
        content = content[:-3]

    content = content.strip()

    # --------------------------------------------------
    # Validate evaluator JSON
    # --------------------------------------------------

    try:

        evaluation = json.loads(content)

    except json.JSONDecodeError:

        return jsonify({
            "error": "Local LLM returned invalid JSON.",
            "raw_response": content
        }), 500

    # --------------------------------------------------
    # Return clean evaluation
    # --------------------------------------------------

    return jsonify(evaluation)


def normalize_algorithmic_answer(answer):
    """Apply the existing evaluator's aviation-number and text normalization."""
    normalized = answer.lower()
    normalized = re.sub(r"\b2000\b", "two thousand", normalized)
    normalized = re.sub(r"\b8000\b", "eight thousand", normalized)
    normalized = re.sub(r"[\W_]+", " ", normalized)
    return " ".join(normalized.split())


@app.route("/algorithmic_evaluate", methods=["POST"])
def algorithmic_evaluate():
    data = request.get_json(silent=True)

    if not data:
        return jsonify({"error": "No JSON data received."}), 400

    expected_answer = data.get("expected_answer", "")
    student_answer = data.get("student_answer", "")

    if not isinstance(expected_answer, str) or not isinstance(student_answer, str):
        return jsonify({"error": "Answers must be strings."}), 400

    normalized_expected = normalize_algorithmic_answer(expected_answer)
    normalized_student = normalize_algorithmic_answer(student_answer)

    similarity = (
        fuzz.ratio(normalized_expected, normalized_student)
        if normalized_expected and normalized_student
        else 0.0
    )
    score = round(similarity)
    correctness = score >= 85

    return jsonify({
        "score": score,
        "correctness": correctness,
        "severity": "",
        "reason": f"RapidFuzz ratio: {similarity:.2f}%",
    })


@app.route("/rapidfuzz_compare", methods=["POST"])
def rapidfuzz_compare():
    data = request.get_json(silent=True)

    if not data:
        return jsonify({"error": "No JSON data received."}), 400

    string_a = data.get("string_a", "")
    string_b = data.get("string_b", "")
    scorer_name = data.get("scorer", "ratio")

    if not isinstance(string_a, str) or not isinstance(string_b, str):
        return jsonify({"error": "String A and String B must be strings."}), 400

    scorers = {
        "ratio": fuzz.ratio,
        "partial_ratio": fuzz.partial_ratio,
        "token_sort_ratio": fuzz.token_sort_ratio,
        "token_set_ratio": fuzz.token_set_ratio,
        "weighted_ratio": fuzz.WRatio,
        "quick_ratio": fuzz.QRatio,
    }
    scorer = scorers.get(scorer_name)
    if scorer is None:
        return jsonify({"error": f"Unsupported RapidFuzz scorer: {scorer_name}"}), 400

    score = scorer(string_a, string_b)
    return jsonify({"scorer": scorer_name, "score": score})


@app.route("/health", methods=["GET"])
def health():
    return jsonify({"status": "ready"})


# --------------------------------------------------
# Start
# --------------------------------------------------

if __name__ == "__main__":

    print("")
    print("======================================")
    print("DGCA RTR LOCAL EVALUATOR")
    print("======================================")
    print("")
    print("Local LLM:")
    print(LLAMA_SERVER_URL)
    print("")
    print("Evaluator:")
    print("http://127.0.0.1:5000/evaluate")
    print("")
    print("Waiting for requests...")
    print("")

    app.run(
        host="127.0.0.1",
        port=5000,
        debug=False
    )
