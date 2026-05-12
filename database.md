# Database Documentation

# DATABASE NAME

VietnameseLearningPlatform

--------------------------------------------------
# MAIN TABLES
--------------------------------------------------

## Users
Stores:
- authentication
- profile
- XP
- streak

## Courses
Stores:
- course information
- levels
- metadata

## Units
Stores:
- course sections

## Lessons
Stores:
- lesson content
- lesson types

## Vocabulary
Stores:
- words
- IPA
- audio
- examples

## GrammarTopics
Stores:
- grammar explanations

## Quizzes
Stores:
- quiz metadata

## Questions
Stores:
- quiz questions

## Answers
Stores:
- answer options

## UserProgress
Stores:
- lesson progress
- completion tracking

## UserVocabulary
Stores:
- spaced repetition
- familiarity
- mastery

## Videos
Stores:
- learning videos

## Transcripts
Stores:
- subtitle timestamps

## SpeakingAttempts
Stores:
- pronunciation scores
- fluency scores

--------------------------------------------------
# RELATIONSHIPS
--------------------------------------------------

Courses
→ Units
→ Lessons
→ Quizzes
→ Questions
→ Answers

Users
→ UserProgress
→ UserVocabulary
→ UserQuiz

Videos
→ Transcripts

## Auth extensions (JWT refresh, avatar, password reset)

Run `Database/Scripts/2026_auth_extensions.sql` on the SQL Server database used by the API connection string before using refresh tokens, avatar URLs, or password-reset flows. The script adds columns on `Users` and creates `RefreshTokens`.