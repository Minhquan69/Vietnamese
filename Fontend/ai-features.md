# AI Features Plan

## AI Product Principles

- AI should help learners practice more, understand mistakes faster, and stay motivated.
- AI should not silently publish official lessons without review.
- AI feedback should be short, encouraging, concrete, and level-aware.
- AI should support foreigners with English-first explanations and progressively more Vietnamese.

## AI Architecture

Recommended backend abstraction:

```text
Application/AI/
  AiTutorService
  AiPronunciationService
  AiExerciseGenerator
  AiRecommendationService
  AiSafetyService

Infrastructure/AI/
  OpenAiClient
  PromptTemplates
  AiUsageLogger
```

Frontend feature areas:

```text
features/ai-tutor/
features/pronunciation/
features/recommendations/
features/writing-feedback/
```

## Feature 1: AI Vietnamese Tutor

Learner can ask questions like:

- "Why does this sentence use la?"
- "How do I pronounce nguoi?"
- "Give me travel phrases for ordering coffee."

Backend responsibilities:

- Add learner level and recent mistakes as context.
- Limit answer length.
- Return structured response: explanation, examples, practice prompt.

Frontend responsibilities:

- Chat UI.
- Suggested prompts.
- Save useful answers.
- Convert examples into practice cards.

## Feature 2: AI Pronunciation Feedback

Flow:

1. Learner records audio.
2. Frontend uploads audio attempt.
3. Backend stores attempt metadata.
4. Speech/AI service scores pronunciation and tones.
5. Learner sees feedback and retry suggestions.

Feedback dimensions:

- Tone accuracy.
- Vowel/consonant accuracy.
- Fluency.
- Word stress/rhythm.
- Suggested mouth/tongue position.

## Feature 3: AI Exercise Generator

Use cases:

- Generate extra multiple-choice questions from a unit.
- Generate sentence ordering tasks.
- Generate fill-in-the-blank tasks.
- Generate listening comprehension questions from transcript.

Safety rule:

- Generated exercises start as drafts.
- Admin reviews before publishing.
- Store source unit, prompt, model, and reviewer.

## Feature 4: AI Writing Correction

Learner writes Vietnamese sentence or paragraph.

AI returns:

- Corrected Vietnamese.
- Tone mark corrections.
- Grammar explanation.
- Natural alternative.
- Practice suggestion.

## Feature 5: AI Roleplay

Scenarios:

- Ordering food.
- Taking a taxi.
- Asking directions.
- Meeting a friend.
- Doctor visit.
- Workplace introduction.

AI should adapt difficulty to learner level and use constrained vocabulary for beginners.

## Feature 6: Personalized Recommendations

Inputs:

- Quiz mistakes.
- User progress.
- Time since last review.
- Weak skills.
- Placement test result.
- Pronunciation scores.

Outputs:

- Next lesson.
- Review queue.
- Suggested practice type.
- Skill warnings.

## Required AI Tables

- `AiConversation`
- `AiMessage`
- `AiFeedback`
- `AiRecommendation`
- `AiGeneratedContentDraft`
- `PronunciationAttempt`
- `PronunciationScore`

## Security And Cost Controls

- Never expose AI provider keys to frontend.
- Add rate limits per user.
- Log token usage.
- Cache repeat explanations where possible.
- Redact personal data from prompts.
- Add moderation for user-submitted text.
- Add fallback messages when AI is unavailable.
