import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import type { NavLinkRenderProps } from 'react-router-dom';
import { MdOutlineHealthAndSafety } from 'react-icons/md';
import { useAuth } from '../../core/use-auth';

function navLinkClass({ isActive }: NavLinkRenderProps): string {
  const base = 'rounded-full px-3 py-[0.35rem] text-[0.9rem] font-semibold no-underline';
  return isActive
    ? `${base} bg-blue text-white`
    : `${base} text-muted hover:bg-white/10 hover:no-underline`;
}

export function AppLayout() {
  const auth = useAuth();
  const navigate = useNavigate();

  function logout(): void {
    auth.logout();
    navigate('/login');
  }

  return (
    <>
      <header className="flex items-center gap-3 border-b border-border bg-card px-6 py-[0.9rem]">
        <span className="font-serif text-[1.15rem] font-semibold text-ink">
          <MdOutlineHealthAndSafety className="mr-[0.35rem] inline-block align-[-0.15em] text-blue" />
          HealthTracker
        </span>
        <nav className="ml-3 flex gap-[0.3rem]">
          <NavLink to="/" end className={navLinkClass}>
            Water
          </NavLink>
          <NavLink to="/schedule" className={navLinkClass}>
            Schedule
          </NavLink>
          <NavLink to="/calories" className={navLinkClass}>
            Calories
          </NavLink>
        </nav>
        <span className="flex-1"></span>
        <span className="text-muted">{auth.email}</span>
        <button className="btn-link" onClick={logout}>
          Log out
        </button>
      </header>

      <Outlet />
    </>
  );
}
