import { useState } from 'react';
import type { SubmitEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../core/use-auth';

export function Register() {
  const auth = useAuth();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit(event: SubmitEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await auth.register(email, password);
    } catch {
      setError('Registration failed. Check the email format and password rules.');
      setLoading(false);
      return;
    }

    // Sikeres regisztráció után rögtön be is jelentkeztetjük.
    try {
      await auth.login(email, password);
      navigate('/');
    } catch {
      navigate('/login');
    }
  }

  return (
    <div className="card">
      <h1>💧 HealthTracker</h1>
      <h2>Create account</h2>

      <form onSubmit={submit}>
        <label>
          Email
          <input
            type="email"
            name="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            autoComplete="username"
          />
        </label>
        <label>
          Password
          <input
            type="password"
            name="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="new-password"
          />
        </label>

        <p className="hint">
          Min. 6 characters, with upper- &amp; lowercase, a digit and a symbol (e.g.{' '}
          <code>Passw0rd!</code>).
        </p>

        {error && <p className="error">{error}</p>}

        <button type="submit" disabled={loading}>
          {loading ? 'Creating…' : 'Register'}
        </button>
      </form>

      <p className="muted">
        Already registered? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
