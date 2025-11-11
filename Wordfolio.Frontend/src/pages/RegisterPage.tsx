import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ApiError } from '../api/authApi';
import { useRegisterMutation } from '../mutations/useRegisterMutation';
import { usePasswordRequirementsQuery } from '../queries/usePasswordRequirementsQuery';
import { createRegisterSchema, RegisterFormData } from '../schemas/authSchemas';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Skeleton,
  Link as MuiLink,
} from '@mui/material';
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
    <Container maxWidth="sm" className="register-container">
      <Paper elevation={3} className="register-paper">
        <Typography variant="h4" component="h1" gutterBottom align="center">
          Register
        </Typography>
        {isLoadingRequirements ? (
          <Box className="register-loading">
            <Skeleton variant="rectangular" height={56} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={56} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={56} sx={{ mb: 2 }} />
            <Skeleton variant="rectangular" height={42} />
          </Box>
        ) : (
          <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
            <TextField
              fullWidth
              margin="normal"
              id="email"
              label="Email"
              type="email"
              autoComplete="email"
              error={!!errors.email}
              helperText={errors.email?.message ?? ' '}
              {...register('email')}
            />

            <TextField
              fullWidth
              margin="normal"
              id="password"
              label="Password"
              type="password"
              autoComplete="new-password"
              error={!!errors.password}
              helperText={errors.password?.message ?? ' '}
              {...register('password')}
            />

            <TextField
              fullWidth
              margin="normal"
              id="confirmPassword"
              label="Confirm Password"
              type="password"
              autoComplete="new-password"
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword?.message ?? ' '}
              {...register('confirmPassword')}
            />

            <Button
              fullWidth
              type="submit"
              variant="contained"
              size="large"
              disabled={registerMutation.isPending}
              className="register-button"
            >
              {registerMutation.isPending ? 'Registering...' : 'Register'}
            </Button>

            <Box className="register-footer">
              <Typography variant="body2">
                Already have an account?{' '}
                <MuiLink component={Link} to="/login" underline="hover">
                  Login here
                </MuiLink>
              </Typography>
            </Box>
          </Box>
        )}
      </Paper>
    </Container>
  );
}
