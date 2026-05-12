export interface AiTutorChatRequest {
  conversationId?: number | null;
  message: string;
  scenarioKey?: string | null;
}

export interface AiTutorChatResponse {
  conversationId: number;
  assistantMessage: string;
  suggestions: string[];
}

export interface TutorConversationSummary {
  conversationId: number;
  title: string;
  scenarioKey?: string | null;
  updatedUtc: string;
}

export interface TutorMessage {
  messageId: number;
  role: string;
  content: string;
  createdUtc: string;
}

export const AI_TUTOR_SCENARIOS: { key: string; label: string }[] = [
  { key: 'daily', label: 'Daily life' },
  { key: 'restaurant', label: 'Restaurant' },
  { key: 'airport', label: 'Airport' },
  { key: 'hotel', label: 'Hotel' },
  { key: 'shopping', label: 'Shopping' },
];
