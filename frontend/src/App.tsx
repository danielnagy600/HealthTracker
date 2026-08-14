import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './core/RequireAuth';
import { Dashboard } from './features/dashboard/Dashboard';
import { Login } from './features/login/Login';
import { Register } from './features/register/Register';

/** Az alkalmazás útvonalai (az Angular `app.routes.ts` megfelelője). */
export function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <RequireAuth>
            <Dashboard />
          </RequireAuth>
        }
      />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
