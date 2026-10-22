import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from './core/authorization/RequireAuth';
import { Calories } from './pages/calories/Calories';
import { AppLayout } from './pages/layout/AppLayout';
import { Login } from './pages/login/Login';
import { Register } from './pages/register/Register';
import { Schedule } from './pages/schedule/Schedule';
import { WaterTracker } from './pages/water-tracker/WaterTracker';

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
        <Route path="/" element={<WaterTracker />} />
        <Route path="/schedule" element={<Schedule />} />
        <Route path="/calories" element={<Calories />} />
      </Route>

      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
