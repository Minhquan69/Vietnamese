export interface UserResult {
  userId: number;
  email: string;
  name: string;
  role: string;
  status?: number;
  avatarUrl?: string | null;
}
