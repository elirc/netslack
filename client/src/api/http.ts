import type { Channel, ChannelType, Message, TokenResponse, Workspace } from "./types";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7208";

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly traceId?: string
  ) {
    super(message);
  }
}

export class ApiClient {
  constructor(private readonly getAccessToken: () => string | null) {}

  async devToken(email: string, displayName: string): Promise<TokenResponse> {
    return this.request<TokenResponse>("/api/auth/dev-token", {
      method: "POST",
      body: { email, displayName },
      anonymous: true
    });
  }

  async getWorkspaces(): Promise<Workspace[]> {
    return this.request<Workspace[]>("/api/workspaces");
  }

  async createWorkspace(name: string, slug: string): Promise<Workspace> {
    return this.request<Workspace>("/api/workspaces", {
      method: "POST",
      body: { name, slug }
    });
  }

  async getChannels(workspaceId: string): Promise<Channel[]> {
    return this.request<Channel[]>(`/api/workspaces/${workspaceId}/channels`);
  }

  async createChannel(workspaceId: string, name: string, topic: string, type: ChannelType): Promise<Channel> {
    return this.request<Channel>(`/api/workspaces/${workspaceId}/channels`, {
      method: "POST",
      body: { name, topic, type }
    });
  }

  async getMessages(channelId: string, take = 50): Promise<Message[]> {
    return this.request<Message[]>(`/api/channels/${channelId}/messages?take=${take}`);
  }

  async postMessage(channelId: string, body: string): Promise<Message> {
    return this.request<Message>(`/api/channels/${channelId}/messages`, {
      method: "POST",
      body: { body }
    });
  }

  private async request<T>(path: string, options: RequestOptions = {}): Promise<T> {
    const headers = new Headers({ Accept: "application/json" });
    if (options.body !== undefined) {
      headers.set("Content-Type", "application/json");
    }

    const token = this.getAccessToken();
    if (!options.anonymous && token) {
      headers.set("Authorization", `Bearer ${token}`);
    }

    const response = await fetch(`${API_BASE_URL}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body === undefined ? undefined : JSON.stringify(options.body)
    });

    if (!response.ok) {
      const problem = await safeReadProblem(response);
      throw new ApiError(problem.message, response.status, problem.traceId);
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }
}

type RequestOptions = {
  method?: "GET" | "POST" | "PUT" | "PATCH" | "DELETE";
  body?: unknown;
  anonymous?: boolean;
};

async function safeReadProblem(response: Response): Promise<{ message: string; traceId?: string }> {
  try {
    const body = (await response.json()) as { title?: string; detail?: string; traceId?: string };
    return {
      message: body.detail ?? body.title ?? `Request failed with ${response.status}`,
      traceId: body.traceId
    };
  } catch {
    return { message: `Request failed with ${response.status}` };
  }
}

export { API_BASE_URL };

