import { useAuthStore } from '../stores/authStore';
import { useNavigate, Link } from '@tanstack/react-router';
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
        <div className="content">
          <p>Please log in to continue.</p>
          <div className="auth-links">
            <Link to="/login" className="auth-link">
              Login
            </Link>
            <Link to="/register" className="auth-link">
              Register
            </Link>
          </div>
        </div>
      )}
    </div>
  );
}
