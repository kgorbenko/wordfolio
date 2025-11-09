import { useState, FormEvent } from 'react';
import { useNavigate, Link } from '@tanstack/react-router';
import { ApiError } from '../api/authApi';
import { useAuthStore } from '../stores/authStore';
import { useLoginMutation } from '../mutations/useLoginMutation';
import './LoginPage.css';

export function LoginPage() {
  const navigate = useNavigate();
  const setTokens = useAuthStore((state) => state.setTokens);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const loginMutation = useLoginMutation({
    onSuccess: (data) => {
      setTokens(data);
      navigate({ to: '/' });
    },
    onError: (error: ApiError) => {
      if (error.status === 401) {
        setError('Invalid email or password');
      } else {
        setError('An error occurred. Please try again.');
      }
    },
  });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setError('');
    loginMutation.mutate({ email, password });
  };

  return (
    <div className="login-page">
      <h1>Login</h1>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="email">Email:</label>
          <input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>

        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>

        {error && <div className="error-message">{error}</div>}

        <button type="submit" disabled={loginMutation.isPending} className="submit-button">
          {loginMutation.isPending ? 'Logging in...' : 'Login'}
        </button>
      </form>

      <div className="footer-link">
        Don't have an account? <Link to="/register">Register here</Link>
      </div>
    </div>
  );
}
