import { useState, FormEvent } from 'react';
import { useNavigate, Link } from '@tanstack/react-router';
import { ApiError } from '../api/authApi';
import { useRegisterMutation } from '../mutations/useRegisterMutation';
import './RegisterPage.css';

export function RegisterPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [validationError, setValidationError] = useState('');

  const registerMutation = useRegisterMutation({
    onSuccess: () => {
      navigate({ to: '/login' });
    },
    onError: (error: ApiError) => {
      if (error.errors) {
        setErrors(error.errors);
      }
    },
  });

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setErrors({});
    setValidationError('');

    if (password !== confirmPassword) {
      setValidationError('Passwords do not match');
      return;
    }

    if (password.length < 6) {
      setValidationError('Password must be at least 6 characters long');
      return;
    }

    registerMutation.mutate({ email, password });
  };

  return (
    <div className="register-page">
      <h1>Register</h1>
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

        <div className="form-group">
          <label htmlFor="confirmPassword">Confirm Password:</label>
          <input
            id="confirmPassword"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </div>

        {validationError && <div className="error-message">{validationError}</div>}

        {Object.keys(errors).length > 0 && (
          <div className="error-message">
            {Object.entries(errors).map(([key, messages]) => (
              <div key={key}>
                {messages.map((msg, idx) => (
                  <div key={idx}>{msg}</div>
                ))}
              </div>
            ))}
          </div>
        )}

        <button type="submit" disabled={registerMutation.isPending} className="submit-button">
          {registerMutation.isPending ? 'Registering...' : 'Register'}
        </button>
      </form>

      <div className="footer-link">
        Already have an account? <Link to="/login">Login here</Link>
      </div>
    </div>
  );
}
