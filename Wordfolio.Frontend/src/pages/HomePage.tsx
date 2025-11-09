import { useAuthStore } from '../stores/authStore';
import { useNavigate } from '@tanstack/react-router';

export function HomePage() {
  const { isAuthenticated, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate({ to: '/login' });
  };

  return (
    <div style={{ maxWidth: '800px', margin: '50px auto', padding: '20px' }}>
      <h1>Welcome to Wordfolio</h1>
      {isAuthenticated ? (
        <div>
          <p>You are logged in!</p>
          <button
            onClick={handleLogout}
            style={{
              padding: '10px 20px',
              backgroundColor: '#dc3545',
              color: 'white',
              border: 'none',
              cursor: 'pointer',
            }}
          >
            Logout
          </button>
        </div>
      ) : (
        <p>Please log in to continue.</p>
      )}
    </div>
  );
}
