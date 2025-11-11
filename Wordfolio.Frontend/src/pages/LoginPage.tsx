import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ApiError } from '../api/authApi';
import { useAuthStore } from '../stores/authStore';
import { useLoginMutation } from '../mutations/useLoginMutation';
import { createLoginSchema, LoginFormData } from '../schemas/authSchemas';
import './LoginPage.css';

export function LoginPage() {
  const navigate = useNavigate();
  const setTokens = useAuthStore((state) => state.setTokens);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(createLoginSchema()),
  });

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
      <form onSubmit={handleSubmit(onSubmit)} noValidate>
        <div className="form-group">
          <label htmlFor="email">Email:</label>
          <input
            id="email"
            type="email"
            {...register('email')}
          />
          {errors.email && <div className="error-message">{errors.email.message}</div>}
        </div>

        <div className="form-group">
          <label htmlFor="password">Password:</label>
          <input
            id="password"
            type="password"
            {...register('password')}
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
