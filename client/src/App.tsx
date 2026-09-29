import { useCallback, useMemo, useState } from "react";
import { ApiClient } from "./api/http";
import type { AuthUser, TokenResponse } from "./api/types";
import { ErrorBoundary } from "./components/ErrorBoundary";
import { WorkspaceShell } from "./features/workspace/WorkspaceShell";

const authStorageKey = "netslack.auth";

export function App() {
  const [auth, setAuth] = useState<TokenResponse | null>(() => readAuth());
  const api = useMemo(() => new ApiClient(() => auth?.accessToken ?? null), [auth?.accessToken]);

  const signIn = useCallback(async (email: string, displayName: string) => {
    const response = await api.devToken(email, displayName);
    localStorage.setItem(authStorageKey, JSON.stringify(response));
    setAuth(response);
  }, [api]);

  const signOut = useCallback(() => {
    localStorage.removeItem(authStorageKey);
    setAuth(null);
  }, []);

  return (
    <ErrorBoundary>
      {auth ? (
        <WorkspaceShell api={api} user={auth.user} accessToken={auth.accessToken} onSignOut={signOut} />
      ) : (
        <SignIn onSignIn={signIn} />
      )}
    </ErrorBoundary>
  );
}

function SignIn({ onSignIn }: { onSignIn: (email: string, displayName: string) => Promise<void> }) {
  const [email, setEmail] = useState("ada@netslack.dev");
  const [displayName, setDisplayName] = useState("Ada Lovelace");
  const [isSubmitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await onSignIn(email, displayName);
    } catch (signInError) {
      setError(signInError instanceof Error ? signInError.message : "Sign in failed.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <main className="center-screen">
      <form className="auth-panel" onSubmit={handleSubmit}>
        <p className="eyebrow">NetSlack</p>
        <h1>Sign in to your workspace</h1>
        <label>
          Display name
          <input value={displayName} onChange={(event) => setDisplayName(event.target.value)} />
        </label>
        <label>
          Email
          <input type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
        </label>
        {error && <p className="form-error">{error}</p>}
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Signing in..." : "Continue"}
        </button>
      </form>
    </main>
  );
}

function readAuth(): TokenResponse | null {
  const raw = localStorage.getItem(authStorageKey);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as TokenResponse & { user?: AuthUser };
    return parsed.accessToken && parsed.user ? parsed : null;
  } catch {
    localStorage.removeItem(authStorageKey);
    return null;
  }
}
