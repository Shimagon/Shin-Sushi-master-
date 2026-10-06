export interface User {
  id: string;
  username: string;
  avatar_url: string | null;
  points: number;
  invited_by: string | null;
  created_at: string;
}

export interface InvitationCode {
  code: string;
  created_by: string;
  is_used: boolean;
  used_by: string | null;
  created_at: string;
}

export interface ChatMessage {
  id: string;
  user_id: string;
  username: string;
  avatar_url: string | null;
  content: string;
  timestamp: Date;
}

export interface OnlineUser {
  id: string;
  username: string;
  avatar_url: string | null;
}
