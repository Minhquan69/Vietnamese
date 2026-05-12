export interface ApiEnvelope<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  /** Legacy alias for access token */
  token: string;
  expiresIn: number;
}

export interface ProfileDto {
  userId: number;
  name: string;
  email: string;
  role: string;
  status: number;
  avatarUrl?: string | null;
}
