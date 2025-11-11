import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ApiError } from '../api/authApi';
import { useRegisterMutation } from '../mutations/useRegisterMutation';
import { usePasswordRequirementsQuery } from '../queries/usePasswordRequirementsQuery';
import { createRegisterSchema, RegisterFormData } from '../schemas/authSchemas';
import './RegisterPage.css';

export function RegisterPage() {
  const navigate = useNavigate();
  const { data: passwordRequirements, isLoading: isLoadingRequirements } = usePasswordRequirementsQuery();

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: passwordRequirements ? zodResolver(createRegisterSchema(passwordRequirements)) : undefined,
  });

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
    registerMutation.mutate({ email: data.email, password: data.password });
  };

  return (
    <div className="register-page">
      <h1>Register</h1>
      {isLoadingRequirements ? (
        <div>Loading...</div>
      ) : (
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

          <div className="form-group">
            <label htmlFor="confirmPassword">Confirm Password:</label>
            <input
              id="confirmPassword"
              type="password"
              {...register('confirmPassword')}
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
