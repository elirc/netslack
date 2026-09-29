import { Component, type ErrorInfo, type ReactNode } from "react";

type Props = {
  children: ReactNode;
};

type State = {
  error?: Error;
};

export class ErrorBoundary extends Component<Props, State> {
  state: State = {};

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Unhandled React error", error, errorInfo);
  }

  render() {
    if (this.state.error) {
      return (
        <main className="center-screen">
          <section className="auth-panel">
            <p className="eyebrow">Something broke</p>
            <h1>NetSlack hit an unexpected UI error.</h1>
            <p className="muted">{this.state.error.message}</p>
            <button type="button" onClick={() => window.location.reload()}>Reload</button>
          </section>
        </main>
      );
    }

    return this.props.children;
  }
}

