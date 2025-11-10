import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { ApiError } from '../api/authApi';
import { useAuthStore } from '../stores/authStore';
import { useLoginMutation } from '../mutations/useLoginMutation';
import './LoginPage.css';

interface LoginFormData {
  email: string;
  password: string;
}

export function LoginPage() {
  const navigate = useNavigate();
  const setTokens = useAuthStore((state) => state.setTokens);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormData>();

  const loginMutation = useLoginMutation({
    onSuccess: (data) => {
      setTokens(data);
      navigate({ to: '/' });
    },
    onError: (error: ApiError) => {
      if (error.status === 401) {
        setError('root', {
          type: 'manual',
          message: 'Invalid email or password',
        });
      } else {
        setError('root', {
          type: 'manual',
          message: 'An error occurred. Please try again.',
        });
      }
    },
  });

  const onSubmit = (data: LoginFormData) => {
    loginMutation.mutate({ email: data.email, password: data.password });
  };

  return (
    <div className="login-page">
      <h1>Login</h1>
      <form onSubmit={handleSubmit(onSubmit)}>
        <div className="form-group">
          <label htmlFor="email">Email:</label>
          <input
            id="email"
            type="email"
            {...register('email', { required: 'Email is required' })}
          />
          {errors.email && <div className="error-message">{errors.email.message}</div>}
        </div>

        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            id="password"
            type="password"
            {...register('password', { required: 'Password is required' })}
          />
          {errors.password && <div className="error-message">{errors.password.message}</div>}
        </div>

        {errors.root && <div className="error-message">{errors.root.message}</div>}

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
