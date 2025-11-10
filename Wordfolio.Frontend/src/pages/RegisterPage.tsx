import { useState, FormEvent } from 'react';
import { useNavigate, Link } from '@tanstack/react-router';
import { ApiError } from '../api/authApi';
import { useRegisterMutation } from '../mutations/useRegisterMutation';
import { usePasswordRequirementsQuery } from '../queries/usePasswordRequirementsQuery';
import './RegisterPage.css';

export function RegisterPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [errors, setErrors] = useState<Record<string, string[]>>({});
  const [validationError, setValidationError] = useState('');

  const { data: passwordRequirements } = usePasswordRequirementsQuery();

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

  const validatePassword = (pwd: string): string | null => {
    if (!passwordRequirements) {
      return null;
    }

    if (pwd.length < passwordRequirements.requiredLength) {
      return `Password must be at least ${passwordRequirements.requiredLength} characters long`;
    }

    if (passwordRequirements.requireDigit && !/\d/.test(pwd)) {
      return 'Password must contain at least one digit';
    }

    if (passwordRequirements.requireLowercase && !/[a-z]/.test(pwd)) {
      return 'Password must contain at least one lowercase letter';
    }

    if (passwordRequirements.requireUppercase && !/[A-Z]/.test(pwd)) {
      return 'Password must contain at least one uppercase letter';
    }

    if (passwordRequirements.requireNonAlphanumeric && !/[^a-zA-Z0-9]/.test(pwd)) {
      return 'Password must contain at least one non-alphanumeric character';
    }

    if (passwordRequirements.requiredUniqueChars > 0) {
      const uniqueChars = new Set(pwd).size;
      if (uniqueChars < passwordRequirements.requiredUniqueChars) {
        return `Password must contain at least ${passwordRequirements.requiredUniqueChars} unique characters`;
      }
    }

    return null;
  };

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    setErrors({});
    setValidationError('');

    if (password !== confirmPassword) {
      setValidationError('Passwords do not match');
      return;
    }

    const passwordError = validatePassword(password);
    if (passwordError) {
      setValidationError(passwordError);
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

        {passwordRequirements && (
          <div className="password-requirements">
            <p>Password must meet the following requirements:</p>
            <ul>
              <li>At least {passwordRequirements.requiredLength} characters long</li>
              {passwordRequirements.requireDigit && <li>Contains at least one digit</li>}
              {passwordRequirements.requireLowercase && <li>Contains at least one lowercase letter</li>}
              {passwordRequirements.requireUppercase && <li>Contains at least one uppercase letter</li>}
              {passwordRequirements.requireNonAlphanumeric && <li>Contains at least one non-alphanumeric character</li>}
              {passwordRequirements.requiredUniqueChars > 0 && (
                <li>Contains at least {passwordRequirements.requiredUniqueChars} unique characters</li>
              )}
            </ul>
          </div>
        )}

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
