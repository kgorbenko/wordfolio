import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { ApiError } from '../api/authApi';
import { useRegisterMutation } from '../mutations/useRegisterMutation';
import { usePasswordRequirementsQuery } from '../queries/usePasswordRequirementsQuery';
import { validatePassword } from '../utils/passwordValidation';
import './RegisterPage.css';

interface RegisterFormData {
  email: string;
  password: string;
  confirmPassword: string;
}

export function RegisterPage() {
  const navigate = useNavigate();
  const { data: passwordRequirements, isLoading: isLoadingRequirements } = usePasswordRequirementsQuery();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterFormData>();

  const registerMutation = useRegisterMutation({
    onSuccess: () => {
      navigate({ to: '/login' });
    },
    onError: (error: ApiError) => {
      if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]) => {
          const fieldName = field.toLowerCase() as keyof RegisterFormData;
          if (fieldName in { email: true, password: true, confirmPassword: true }) {
            setError(fieldName, {
              type: 'server',
              message: messages.join(', '),
            });
          }
        });
      }
    },
  });

  const onSubmit = (data: RegisterFormData) => {
    if (!passwordRequirements) {
      return;
    }

    if (data.password !== data.confirmPassword) {
      setError('confirmPassword', {
        type: 'manual',
        message: 'Passwords do not match',
      });
      return;
    }

    const passwordValidation = validatePassword(data.password, passwordRequirements);
    if (!passwordValidation.isValid) {
      setError('password', {
        type: 'manual',
        message: passwordValidation.message,
      });
      return;
    }

    registerMutation.mutate({ email: data.email, password: data.password });
  };

  return (
    <div className="register-page">
      <h1>Register</h1>
      {isLoadingRequirements ? (
        <div>Loading...</div>
      ) : (
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

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password:</label>
            <input
              id="confirmPassword"
              type="password"
              {...register('confirmPassword', { required: 'Please confirm your password' })}
            />
            {errors.confirmPassword && <div className="error-message">{errors.confirmPassword.message}</div>}
          </div>

          <button type="submit" disabled={registerMutation.isPending} className="submit-button">
            {registerMutation.isPending ? 'Registering...' : 'Register'}
          </button>
        </form>
      )}

      <div className="footer-link">
        Already have an account? <Link to="/login">Login here</Link>
      </div>
    </div>
  );
}
