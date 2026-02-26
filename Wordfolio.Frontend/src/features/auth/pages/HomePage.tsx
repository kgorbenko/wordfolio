import { useNavigate, Link } from "@tanstack/react-router";
import { Container, Typography, Box, Button } from "@mui/material";

import { useAuthStore } from "../../../stores/authStore";
import { loginPath, registerPath } from "../routes";

import styles from "./HomePage.module.scss";

export const HomePage = () => {
    const { isAuthenticated, clearAuth } = useAuthStore();
    const navigate = useNavigate();

    const handleLogout = () => {
        clearAuth();
        navigate(loginPath());
    };

    return (
        <Container maxWidth="md" className={styles.container}>
            <Typography variant="h2" component="h1" gutterBottom>
                Welcome to Wordfolio
            </Typography>
            {isAuthenticated ? (
                <Box className={styles.content}>
                    <Typography variant="body1" paragraph>
                        You are logged in!
                    </Typography>
                    <Button
                        variant="contained"
                        color="error"
                        onClick={handleLogout}
                        className={styles.button}
                    >
                        Logout
                    </Button>
                </Box>
            ) : (
                <Box className={styles.content}>
                    <Typography variant="body1" paragraph>
                        Please log in to continue.
                    </Typography>
                    <Box className={styles.authLinks}>
                        <Button
                            component={Link}
                            {...loginPath()}
                            variant="contained"
                            color="primary"
                        >
                            Login
                        </Button>
                        <Button
                            component={Link}
                            {...registerPath()}
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
};
