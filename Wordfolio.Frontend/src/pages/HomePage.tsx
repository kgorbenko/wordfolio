import { useAuthStore } from '../stores/authStore';
import { useNavigate, Link } from '@tanstack/react-router';
import { Container, Typography, Box, Button } from '@mui/material';
import './HomePage.css';

export function HomePage() {
  const { isAuthenticated, clearAuth } = useAuthStore();
  const navigate = useNavigate();

  const handleLogout = () => {
    clearAuth();
    navigate({ to: '/login' });
  };

  return (
    <Container maxWidth="md" className="home-container">
      <Typography variant="h2" component="h1" gutterBottom>
        Welcome to Wordfolio
      </Typography>
      {isAuthenticated ? (
        <Box className="home-content">
          <Typography variant="body1" paragraph>
            You are logged in!
          </Typography>
          <Button
            variant="contained"
            color="error"
            onClick={handleLogout}
            className="home-button"
          >
            Logout
          </Button>
        </Box>
      ) : (
        <Box className="home-content">
          <Typography variant="body1" paragraph>
            Please log in to continue.
          </Typography>
          <Box className="home-auth-links">
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
