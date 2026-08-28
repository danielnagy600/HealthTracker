import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './core/RequireAuth';
import { Calories } from './features/calories/Calories';
import { Dashboard } from './features/dashboard/Dashboard';
import { AppLayout } from './features/layout/AppLayout';
import { Login } from './features/login/Login';
import { Register } from './features/register/Register';
import { Schedule } from './features/schedule/Schedule';

export function App() {
  return (
    <Routes>
      <Route
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route path="/" element={<Dashboard />} />
        <Route path="/schedule" element={<Schedule />} />
        <Route path="/calories" element={<Calories />} />
      </Route>

      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
