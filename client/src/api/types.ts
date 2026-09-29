export type AuthUser = {
  id: string;
  email: string;
  displayName: string;
  avatarUrl: string;
};

export type TokenResponse = {
  accessToken: string;
  user: AuthUser;
};

export type Workspace = {
  id: string;
  name: string;
  slug: string;
  createdAt: string;
};

export type ChannelType = "Public" | "Private" | "DirectMessage";

export type Channel = {
  id: string;
  workspaceId: string;
  name: string;
  topic: string;
  type: ChannelType;
  memberCount: number;
};

export type MessageAuthor = {
  id: string;
  displayName: string;
  email: string;
  avatarUrl: string;
};

export type Message = {
  id: string;
  channelId: string;
  body: string;
  postedAt: string;
  editedAt?: string | null;
  author: MessageAuthor;
};

