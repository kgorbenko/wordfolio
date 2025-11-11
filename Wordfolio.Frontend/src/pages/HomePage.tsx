import { useAuthStore } from '../stores/authStore';
import { useNavigate, Link } from '@tanstack/react-router';
import { Container, Typography, Box, Button } from '@mui/material';

export function HomePage() {
  const { isAuthenticated, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate({ to: '/login' });
  };

  return (
    <Container maxWidth="md" sx={{ mt: 8, textAlign: 'center' }}>
      <Typography variant="h2" component="h1" gutterBottom>
        Welcome to Wordfolio
      </Typography>
      {isAuthenticated ? (
        <Box sx={{ mt: 4 }}>
          <Typography variant="body1" paragraph>
            You are logged in!
          </Typography>
          <Button
            variant="contained"
            color="error"
            onClick={handleLogout}
            sx={{ mt: 2 }}
          >
            Logout
          </Button>
        </Box>
      ) : (
        <Box sx={{ mt: 4 }}>
          <Typography variant="body1" paragraph>
            Please log in to continue.
          </Typography>
          <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center', mt: 3 }}>
            <Button
              component={Link}
              to="/login"
              variant="contained"
              color="primary"
            >
              Login
            </Button>
            <Button
              component={Link}
              to="/register"
              variant="outlined"
              color="primary"
            >
              Register
            </Button>
          </Box>
        </Box>
      )}
    </Container>
  );
}
