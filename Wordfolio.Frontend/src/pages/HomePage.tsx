import { useAuthStore } from '../stores/authStore';
import { useNavigate } from '@tanstack/react-router';
import './HomePage.css';

export function HomePage() {
  const { isAuthenticated, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate({ to: '/login' });
  };

  return (
    <div className="home-page">
      <h1>Welcome to Wordfolio</h1>
      {isAuthenticated ? (
        <div className="content">
          <p>You are logged in!</p>
          <button onClick={handleLogout} className="logout-button">
            Logout
          </button>
        </div>
      ) : (
        <p>Please log in to continue.</p>
      )}
    </div>
  );
}
