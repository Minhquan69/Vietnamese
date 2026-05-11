# Database Improvement Plan

## Current Database Shape

Current EF Core entities:

- `User`, `Role`
- `Level`, `Course`, `Unit`
- `Quiz`, `Part`, `Passage`, `Question`, `Answer`
- `UserQuiz`, `UserAnswer`, `UserProgress`
- `PlacementTest`
- `Video`, `Transcript`

The schema supports course hierarchy, tests, answers, user progress, and transcript search.

## Current Strengths

- Clear hierarchy: Level -> Course -> Unit.
- Quiz can attach to multiple reference types through `RefType` and `RefId`.
- Test content supports parts, passages, questions, and answers.
- Several useful unique indexes already exist.
- User quiz attempts and user answers are modeled.

## Database Risks

- `RefType` + `RefId` is flexible but weakly typed. It can create orphan references.
- Migrations are missing from the tracked source.
- `CreatedBy` is sometimes string and sometimes integer across entities.
- Some required navigation properties are nullable while required scalar fields are not consistently initialized.
- No audit fields across most tables.
- No soft-delete strategy.
- No content publishing status beyond simple `IsActive`.
- No vocabulary, grammar, pronunciation, spaced repetition, achievement, or AI feedback schema.
- Uploaded media paths are stored directly without a media metadata table.

## Recommended Core Additions

### Content Metadata

Add common fields:

- `CreatedAt`
- `UpdatedAt`
- `CreatedByUserId`
- `UpdatedByUserId`
- `Status`: Draft, Review, Published, Archived
- `DeletedAt` for soft delete where needed

### Skill Taxonomy

Add:

- `Skill`
  - Pronunciation
  - Tone
  - Vocabulary
  - Grammar
  - Listening
  - Speaking
  - Reading
  - Writing
- `UnitSkill`
- `QuestionSkill`

### Vocabulary

Add:

- `VocabularyItem`
- `VocabularyExample`
- `VocabularyAudio`
- `VocabularyTranslation`
- `UserVocabularyProgress`

### Grammar

Add:

- `GrammarPoint`
- `GrammarExample`
- `UnitGrammarPoint`
- `UserGrammarProgress`

### Spaced Repetition

Add:

- `ReviewItem`
- `ReviewSchedule`
- `ReviewAttempt`

Suggested fields:

- `EaseFactor`
- `IntervalDays`
- `DueAt`
- `LastReviewedAt`
- `CorrectStreak`
- `IncorrectCount`

### Pronunciation

Add:

- `PronunciationPrompt`
- `PronunciationAttempt`
- `PronunciationScore`

Suggested score dimensions:

- Accuracy
- Tone accuracy
- Fluency
- Rhythm
- Final consonant accuracy
- AI feedback text

### AI

Add:

- `AiConversation`
- `AiMessage`
- `AiFeedback`
- `AiRecommendation`
- `AiGeneratedContentDraft`

Store prompt metadata and model provider metadata for traceability, but do not store raw secrets.

### Achievements

Add:

- `Achievement`
- `UserAchievement`
- `UserStreak`
- `Certificate`

## Migration Strategy

1. Restore EF Core migrations as source-controlled artifacts.
2. Create a clean baseline migration from the current production schema.
3. Add schema changes incrementally per feature.
4. Never manually edit generated migration files unless required and reviewed.
5. Back up SQL Server before destructive migrations.
6. Seed roles, default levels, and starter content through controlled seed scripts.

## Index Strategy

Add indexes for:

- `User.Email`
- `Transcript.Sentence` full-text search if SQL Server full-text is available.
- `UserProgress(UserId, RefType, RefId)`
- `ReviewSchedule(UserId, DueAt)`
- `PronunciationAttempt(UserId, PromptId, CreatedAt)`
- `AiRecommendation(UserId, CreatedAt)`

## Data Quality Rules

- Avoid storing display order gaps as a problem; allow reorder operations to normalize.
- Enforce unique email.
- Enforce one correct answer minimum per question where question type requires it.
- Store media metadata separately from content records.
- Use enums or lookup tables for status/ref type to reduce invalid strings.
