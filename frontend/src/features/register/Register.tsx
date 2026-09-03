import { useState } from 'react';
import type { SubmitEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { MdOutlineHealthAndSafety } from 'react-icons/md';
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

    try {
      await auth.login(email, password);
      navigate('/');
    } catch {
      navigate('/login');
    }
  }

  return (
    <div className="mx-auto my-[8vh] max-w-[380px] rounded-2xl border border-border-strong bg-card p-8 shadow-[0_30px_60px_-20px_rgb(0_0_0_/_65%)]">
      <h1 className="mb-1 font-serif text-2xl font-semibold text-ink">
        <MdOutlineHealthAndSafety className="mr-[0.35rem] inline-block align-[-0.15em] text-blue" />
        HealthTracker
      </h1>
      <h2 className="mb-5 font-serif text-[1.05rem] font-medium text-muted italic">Create account</h2>

      <form className="flex flex-col gap-[0.9rem]" onSubmit={submit}>
        <label className="flex flex-col gap-[0.35rem] text-[0.85rem] text-muted">
          Email
          <input
            className="field"
            type="email"
            name="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            required
            autoComplete="username"
          />
        </label>
        <label className="flex flex-col gap-[0.35rem] text-[0.85rem] text-muted">
          Password
          <input
            className="field"
            type="password"
            name="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            required
            autoComplete="new-password"
          />
        </label>

        <p className="-mt-[0.3rem] text-[0.78rem] text-muted">
          Min. 6 characters, with upper- &amp; lowercase, a digit and a symbol (e.g.{' '}
          <code>Passw0rd!</code>).
        </p>

        {error && <p className="m-0 text-[0.85rem] text-red">{error}</p>}

        <button className="btn" type="submit" disabled={loading}>
          {loading ? 'Creating…' : 'Register'}
        </button>
      </form>

      <p className="text-muted">
        Already registered? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
