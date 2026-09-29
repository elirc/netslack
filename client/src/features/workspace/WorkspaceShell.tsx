import { Hash, LogOut, Lock, MessageSquarePlus, Plus, Send } from "lucide-react";
import { useCallback, useEffect, useMemo, useState } from "react";
import type { ApiClient } from "../../api/http";
import type { AuthUser, Channel, ChannelType, Message, Workspace } from "../../api/types";
import { useSignalRChannel } from "../../hooks/useSignalRChannel";

type Props = {
  api: ApiClient;
  user: AuthUser;
  accessToken: string;
  onSignOut: () => void;
};

export function WorkspaceShell({ api, user, accessToken, onSignOut }: Props) {
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [activeWorkspaceId, setActiveWorkspaceId] = useState<string | null>(null);
  const [channels, setChannels] = useState<Channel[]>([]);
  const [activeChannelId, setActiveChannelId] = useState<string | null>(null);
  const [messages, setMessages] = useState<Message[]>([]);
  const [status, setStatus] = useState<LoadStatus>({ kind: "loading", message: "Loading workspace..." });

  const activeWorkspace = useMemo(
    () => workspaces.find((workspace) => workspace.id === activeWorkspaceId) ?? null,
    [activeWorkspaceId, workspaces]
  );

  const activeChannel = useMemo(
    () => channels.find((channel) => channel.id === activeChannelId) ?? null,
    [activeChannelId, channels]
  );

  const appendMessage = useCallback((message: Message) => {
    setMessages((current) => current.some((item) => item.id === message.id) ? current : [...current, message]);
  }, []);

  useSignalRChannel(activeChannelId, accessToken, appendMessage);

  useEffect(() => {
    let disposed = false;

    async function loadWorkspaces() {
      try {
        setStatus({ kind: "loading", message: "Loading workspace..." });
        const result = await api.getWorkspaces();
        if (disposed) {
          return;
        }

        setWorkspaces(result);
        setActiveWorkspaceId((current) => current ?? result[0]?.id ?? null);
        setStatus({ kind: "ready" });
      } catch (error) {
        setStatus({ kind: "error", message: error instanceof Error ? error.message : "Unable to load workspaces." });
      }
    }

    void loadWorkspaces();
    return () => {
      disposed = true;
    };
  }, [api]);

  useEffect(() => {
    if (!activeWorkspaceId) {
      return;
    }

    let disposed = false;

    async function loadChannels() {
      try {
        setStatus({ kind: "loading", message: "Loading channels..." });
        const result = await api.getChannels(activeWorkspaceId!);
        if (disposed) {
          return;
        }

        setChannels(result);
        setActiveChannelId((current) => current && result.some((channel) => channel.id === current) ? current : result[0]?.id ?? null);
        setStatus({ kind: "ready" });
      } catch (error) {
        setStatus({ kind: "error", message: error instanceof Error ? error.message : "Unable to load channels." });
      }
    }

    void loadChannels();
    return () => {
      disposed = true;
    };
  }, [activeWorkspaceId, api]);

  useEffect(() => {
    if (!activeChannelId) {
      setMessages([]);
      return;
    }

    let disposed = false;

    async function loadMessages() {
      try {
        const result = await api.getMessages(activeChannelId!);
        if (!disposed) {
          setMessages(result);
        }
      } catch (error) {
        setStatus({ kind: "error", message: error instanceof Error ? error.message : "Unable to load messages." });
      }
    }

    void loadMessages();
    return () => {
      disposed = true;
    };
  }, [activeChannelId, api]);

  async function createWorkspace() {
    const name = window.prompt("Workspace name");
    if (!name) {
      return;
    }

    const slug = name.toLowerCase().trim().replace(/\s+/g, "-");
    const workspace = await api.createWorkspace(name, slug);
    setWorkspaces((current) => [...current, workspace]);
    setActiveWorkspaceId(workspace.id);
  }

  async function createChannel() {
    if (!activeWorkspaceId) {
      return;
    }

    const name = window.prompt("Channel name");
    if (!name) {
      return;
    }

    const channel = await api.createChannel(activeWorkspaceId, name, "Created from the NetSlack web client", "Public");
    setChannels((current) => [...current, channel]);
    setActiveChannelId(channel.id);
  }

  async function postMessage(body: string) {
    if (!activeChannelId) {
      return;
    }

    const message = await api.postMessage(activeChannelId, body);
    appendMessage(message);
  }

  return (
    <main className="app-shell">
      <aside className="workspace-rail" aria-label="Workspaces">
        {workspaces.map((workspace) => (
          <button
            className={workspace.id === activeWorkspaceId ? "workspace-dot active" : "workspace-dot"}
            key={workspace.id}
            title={workspace.name}
            onClick={() => setActiveWorkspaceId(workspace.id)}
          >
            {workspace.name.slice(0, 2).toUpperCase()}
          </button>
        ))}
        <button className="workspace-dot add" title="Create workspace" onClick={createWorkspace}>
          <Plus size={18} />
        </button>
      </aside>

      <aside className="sidebar">
        <div className="sidebar-header">
          <div>
            <p className="eyebrow">Workspace</p>
            <h2>{activeWorkspace?.name ?? "No workspace"}</h2>
          </div>
          <button className="icon-button" title="Sign out" onClick={onSignOut}>
            <LogOut size={18} />
          </button>
        </div>

        <div className="user-chip">
          <span className="avatar">{initials(user.displayName)}</span>
          <div>
            <strong>{user.displayName}</strong>
            <span>{user.email.toLowerCase()}</span>
          </div>
        </div>

        <div className="section-title">
          <span>Channels</span>
          <button className="icon-button" title="Create channel" onClick={createChannel}>
            <MessageSquarePlus size={17} />
          </button>
        </div>

        <nav className="channel-list" aria-label="Channels">
          {channels.map((channel) => (
            <button
              key={channel.id}
              className={channel.id === activeChannelId ? "channel active" : "channel"}
              onClick={() => setActiveChannelId(channel.id)}
            >
              {channel.type === "Private" ? <Lock size={16} /> : <Hash size={16} />}
              <span>{channel.name}</span>
            </button>
          ))}
        </nav>
      </aside>

      <section className="conversation">
        <header className="conversation-header">
          <div>
            <h1>{activeChannel ? `#${activeChannel.name}` : "Choose a channel"}</h1>
            <p>{activeChannel?.topic ?? "Create a workspace or channel to start talking."}</p>
          </div>
          {activeChannel && <span className="member-pill">{activeChannel.memberCount} members</span>}
        </header>

        {status.kind === "error" && <div className="banner error">{status.message}</div>}
        {status.kind === "loading" && <div className="banner">{status.message}</div>}

        <MessageList messages={messages} />
        <MessageComposer disabled={!activeChannel} onSend={postMessage} />
      </section>
    </main>
  );
}

function MessageList({ messages }: { messages: Message[] }) {
  if (messages.length === 0) {
    return (
      <div className="empty-state">
        <h2>No messages yet</h2>
        <p>Start the conversation with a useful update, decision, or question.</p>
      </div>
    );
  }

  return (
    <ol className="message-list">
      {messages.map((message) => (
        <li key={message.id} className="message">
          <span className="avatar">{initials(message.author.displayName)}</span>
          <div>
            <div className="message-meta">
              <strong>{message.author.displayName}</strong>
              <time dateTime={message.postedAt}>{formatTime(message.postedAt)}</time>
            </div>
            <p>{message.body}</p>
          </div>
        </li>
      ))}
    </ol>
  );
}

function MessageComposer({ disabled, onSend }: { disabled: boolean; onSend: (body: string) => Promise<void> }) {
  const [body, setBody] = useState("");
  const [isSending, setSending] = useState(false);

  async function submit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmed = body.trim();
    if (!trimmed || disabled) {
      return;
    }

    setSending(true);
    try {
      await onSend(trimmed);
      setBody("");
    } finally {
      setSending(false);
    }
  }

  return (
    <form className="composer" onSubmit={submit}>
      <input
        disabled={disabled || isSending}
        value={body}
        onChange={(event) => setBody(event.target.value)}
        placeholder={disabled ? "Choose a channel first" : "Message this channel"}
      />
      <button type="submit" disabled={disabled || isSending || !body.trim()} title="Send message">
        <Send size={18} />
      </button>
    </form>
  );
}

type LoadStatus =
  | { kind: "ready" }
  | { kind: "loading"; message: string }
  | { kind: "error"; message: string };

function initials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join("");
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat(undefined, {
    hour: "numeric",
    minute: "2-digit"
  }).format(new Date(value));
}
