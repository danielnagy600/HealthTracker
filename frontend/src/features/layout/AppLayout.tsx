import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../core/use-auth';

/**
 * A bejelentkezett oldalak közös kerete: fejléc, navigáció, kijelentkezés.
 * A védett útvonalak ezen belül renderelődnek (React Router `<Outlet />`).
 */
export function AppLayout() {
  const auth = useAuth();
  const navigate = useNavigate();

  function logout(): void {
    auth.logout();
    navigate('/login');
  }

  return (
    <>
      <header className="topbar">
        <span className="brand">💧 HealthTracker</span>
        <nav className="mainnav">
          <NavLink to="/" end>
            Water
          </NavLink>
          <NavLink to="/schedule">Schedule</NavLink>
          <NavLink to="/calories">Calories</NavLink>
        </nav>
        <span className="spacer"></span>
        <span className="muted">{auth.email}</span>
        <button className="link" onClick={logout}>
          Log out
        </button>
      </header>

      <Outlet />
    </>
  );
}
