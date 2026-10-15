import os
import json

import spaces
import gradio as gr
from huggingface_hub import InferenceClient
from huggingface_hub import HfApi


# --------------------------------------------------
# ZeroGPU startup requirement
# --------------------------------------------------

@spaces.GPU
def zerogpu_requirement():
    return None


# --------------------------------------------------
# Hugging Face Inference
# --------------------------------------------------

client = InferenceClient(
    provider="auto",
    api_key=os.environ["HF_TOKEN"]
)

try:
    identity = HfApi().whoami(
        token=os.environ["HF_TOKEN"]
    )
    print("HF TOKEN IDENTITY CHECK: SUCCESS")
    print("Authenticated user:", identity.get("name"))
except Exception as e:
    print("HF TOKEN IDENTITY CHECK: FAILED")
    print(type(e).__name__, str(e))
    
# Temporary debugging: verify that the token is loaded.
print("HF_TOKEN present:", bool(os.environ.get("HF_TOKEN")))
print("HF_TOKEN length:", len(os.environ.get("HF_TOKEN", "")))

# --------------------------------------------------
# RTR Evaluation Rules
# --------------------------------------------------

RTR_SYSTEM_PROMPT = """
You are an aviation radiotelephony assessment evaluator
for a DGCA RTR Part 2 training system.

Your task is to evaluate a student's spoken radiotelephony
response against the supplied question and expected answer.

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
   supplied question and expected answer.

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

CORRECTNESS

Set correctness to true only when the student's response is
operationally acceptable for the supplied scenario.

REASONING

Give a concise explanation of the decision. Mention the
important correct, missing, or incorrect operational elements.
Do not provide hidden chain-of-thought; provide only a brief
decision explanation.

OUTPUT FORMAT

Return ONLY valid JSON using exactly this structure:

{
  "correctness": false,
  "reasoning": ""
}

Rules for the fields:

correctness:
true if the student's response is operationally acceptable.
false otherwise.

reasoning:
A concise explanation of why the response is correct or incorrect.

Do not add any other fields.
Do not use Markdown.
Do not use ```json fences.


IMPORTANT OUTPUT BUDGET RULES:

Keep the response concise and within the configured output token limit.

You MUST complete the entire evaluation within this token budget.

You are allowed to reason about the answer, but keep your reasoning concise and focused.
Do not spend excessive tokens exploring possibilities or explaining your reasoning.

After evaluating the student's answer, immediately produce the final result.

Your final response MUST be ONLY valid JSON.

Do not output:
- analysis
- explanations outside JSON
- markdown
- code fences
- additional commentary

The "reasoning" field MUST be concise and no more than 25 words.

Required JSON format:

{
  "correctness": false,
  "reasoning": "Short explanation."
}

The JSON result is the highest priority. Make sure it is generated before the completion token limit is reached.

"""


# --------------------------------------------------
# LLM Evaluation
# --------------------------------------------------

def evaluate_answer(
    question,
    expected_answer,
    student_answer
):

    prompt = f"""
Evaluate the student's response using the evaluation rules.

QUESTION:
{question}

EXPECTED ANSWER:
{expected_answer}

STUDENT ANSWER:
{student_answer}
"""

    response = client.chat_completion(
        model="Qwen/Qwen3-32B",

        messages=[
            {
                "role": "system",
                "content": RTR_SYSTEM_PROMPT
            },
            {
                "role": "user",
                "content": prompt
            }
        ],

        temperature=0.1,
        max_tokens=600,
        extra_body={"reasoning_effort": "low"}
    )

    # --------------------------------------------------
    # Extract LLM result safely
    # --------------------------------------------------

    
    choice = response.choices[0]
    print(
        "LLM response metadata: model=%s, finish_reason=%s" %
        (response.model, choice.finish_reason)
    )

    result = choice.message.content
    # Some provider responses may contain no text content.
    if result is None:
        return json.dumps(
            {
                "correctness": False,
                "reasoning": "The LLM provider returned no text content."
            },
            indent=2
        )

    # --------------------------------------------------
    # Clean accidental Markdown fences
    # --------------------------------------------------

    result = result.strip()

    if result.startswith("```json"):
        result = result[7:]

    if result.endswith("```"):
        result = result[:-3]

    result = result.strip()

    # --------------------------------------------------
    # Validate JSON
    # --------------------------------------------------

    try:
        parsed = json.loads(result)

    except json.JSONDecodeError:
        return json.dumps(
            {
                "correctness": False,
                "reasoning": "The LLM returned an invalid JSON response."
            },
            indent=2
        )

    # --------------------------------------------------
    # Return clean JSON
    # --------------------------------------------------

    if not isinstance(parsed, dict) or not isinstance(parsed.get("correctness"), bool):
        return json.dumps(
            {
                "correctness": False,
                "reasoning": "The LLM response did not contain a valid correctness value."
            },
            indent=2
        )

    # Enforce the two-field output contract even if the model adds extra keys.
    return json.dumps(
        {
            "correctness": parsed["correctness"],
            "reasoning": str(parsed.get("reasoning", ""))
        },
        indent=2
    )


# --------------------------------------------------
# Gradio UI
# --------------------------------------------------

demo = gr.Interface(
    fn=evaluate_answer,

    inputs=[
        gr.Textbox(
            label="Question",
            placeholder="Enter the RTR question..."
        ),

        gr.Textbox(
            label="Expected Answer",
            placeholder="Enter the reference answer..."
        ),

        gr.Textbox(
            label="Student Answer",
            placeholder="Enter the student's spoken/transcribed answer..."
        )
    ],

    outputs=gr.Textbox(
        label="Evaluation JSON"
    ),

    title="DGCA RTR Answer Evaluator",

    description=(
        "Evaluate a student's radiotelephony response "
        "against the expected answer."
    )
)


# --------------------------------------------------
# Start
# --------------------------------------------------

if __name__ == "__main__":
    demo.launch()
