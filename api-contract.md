# API Contract Documentation

# BASE URL

/api/v1

--------------------------------------------------
# AUTH APIs
--------------------------------------------------

POST /auth/register
POST /auth/login
POST /auth/refresh-token
GET /auth/profile

--------------------------------------------------
# COURSE APIs
--------------------------------------------------

GET /courses
GET /courses/{id}
GET /courses/{id}/units
GET /lessons/{id}

--------------------------------------------------
# VOCABULARY APIs
--------------------------------------------------

GET /vocabulary
GET /vocabulary/{id}
POST /vocabulary/save
GET /vocabulary/review

--------------------------------------------------
# QUIZ APIs
--------------------------------------------------

GET /quiz/{id}
POST /quiz/submit
GET /quiz/history

--------------------------------------------------
# AI CHAT APIs
--------------------------------------------------

POST /ai/chat
POST /ai/correct-grammar

--------------------------------------------------
# SPEAKING APIs
--------------------------------------------------

POST /speaking/evaluate
GET /speaking/history

--------------------------------------------------
# VIDEO APIs
--------------------------------------------------

GET /videos
GET /videos/{id}
GET /videos/{id}/transcript

--------------------------------------------------
# RESPONSE FORMAT
--------------------------------------------------

{
  success: boolean,
  message: string,
  data: any,
  errors: []
}