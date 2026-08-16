import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './core/RequireAuth';
import { Dashboard } from './features/dashboard/Dashboard';
import { AppLayout } from './features/layout/AppLayout';
import { Login } from './features/login/Login';
import { Register } from './features/register/Register';
import { Schedule } from './features/schedule/Schedule';

/** Az alkalmazás útvonalai (az Angular `app.routes.ts` megfelelője). */
export function App() {
  return (
    <Routes>
      {/* A védett oldalak közös kereten (fejléc + navigáció) belül élnek. */}
      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route path="/" element={<Dashboard />} />
        <Route path="/schedule" element={<Schedule />} />
      </Route>

      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
