import { useState, type FormEvent } from "react";
import { login } from "../api/authApi";
import { ApiError } from "../api/client";

interface LoginFormProps {
  onLoggedIn: (accessToken: string) => void;
}

export function LoginForm({ onLoggedIn }: LoginFormProps) {
  const [username, setUsername] = useState("demo");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);
    try {
      const token = await login(username, password);
      onLoggedIn(token.accessToken);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Unable to reach the Products API.");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>Sign in</h2>
      <label>
        Username
        <input value={username} onChange={(e) => setUsername(e.target.value)} autoComplete="username" required />
      </label>
      <label>
        Password
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
          required
        />
      </label>
      {error && <p className="error">{error}</p>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Signing in…" : "Sign in"}
      </button>
      <p className="hint">Demo credentials: demo / Demo123!</p>
    </form>
  );
}
