import { useNavigate, Link } from '@tanstack/react-router';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ApiError } from '../api/authApi';
import { useAuthStore } from '../stores/authStore';
import { useLoginMutation } from '../mutations/useLoginMutation';
import { createLoginSchema, LoginFormData } from '../schemas/authSchemas';
import {
  Container,
  Paper,
  Typography,
  TextField,
  Button,
  Box,
  Alert,
  Link as MuiLink,
} from '@mui/material';

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
    <Container maxWidth="sm" sx={{ mt: 8 }}>
      <Paper elevation={3} sx={{ p: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom align="center">
          Login
        </Typography>
        <Box component="form" onSubmit={handleSubmit(onSubmit)} noValidate>
          <TextField
            fullWidth
            margin="normal"
            id="email"
            label="Email"
            type="email"
            autoComplete="email"
            error={!!errors.email}
            helperText={errors.email?.message}
            {...register('email')}
          />

          <TextField
            fullWidth
            margin="normal"
            id="password"
            label="Password"
            type="password"
            autoComplete="current-password"
            error={!!errors.password}
            helperText={errors.password?.message}
            {...register('password')}
          />

          {errors.root && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {errors.root.message}
            </Alert>
          )}

          <Button
            fullWidth
            type="submit"
            variant="contained"
            size="large"
            disabled={loginMutation.isPending}
            sx={{ mt: 3, mb: 2 }}
          >
            {loginMutation.isPending ? 'Logging in...' : 'Login'}
          </Button>

          <Box sx={{ textAlign: 'center', mt: 2 }}>
            <Typography variant="body2">
              Don't have an account?{' '}
              <MuiLink component={Link} to="/register" underline="hover">
                Register here
              </MuiLink>
            </Typography>
          </Box>
        </Box>
      </Paper>
    </Container>
  );
}
