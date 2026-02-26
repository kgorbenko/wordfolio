import { useNavigate, Link } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import {
    Container,
    Typography,
    TextField,
    Button,
    Box,
    Alert,
    Link as MuiLink,
} from "@mui/material";

import { ApiError } from "../../../api/authApi";
import { useAuthStore } from "../../../stores/authStore";
import { useLoginMutation } from "../../../mutations/useLoginMutation";
import { createLoginSchema, LoginFormData } from "../../../schemas/authSchemas";
import { useNotificationContext } from "../../../contexts/NotificationContext";
import { loginRouteApi, homePath, registerPath } from "../routes";

import styles from "./LoginPage.module.scss";

export const LoginPage = () => {
    const navigate = useNavigate();
    const search = loginRouteApi.useSearch();
    const setTokens = useAuthStore((state) => state.setTokens);
    const { openNotification } = useNotificationContext();

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
            navigate(homePath());
        },
        onError: (error: ApiError) => {
            const errorMessage =
                error.status === 401
                    ? "Invalid email or password"
                    : "An error occurred. Please try again.";

            setError("root", {
                type: "server",
                message: errorMessage,
            });
        },
    });

    useEffect(() => {
        if (search.message) {
            openNotification({ message: search.message, severity: "info" });
        }
    }, [search.message, openNotification]);

    const onSubmit = (data: LoginFormData) => {
        loginMutation.mutate({ email: data.email, password: data.password });
    };

    return (
        <div className="centered-page-container">
            <Container maxWidth="sm" className={styles.container}>
                <Typography
                    className={styles.header}
                    variant="h5"
                    component="h1"
                    gutterBottom
                    align="center"
                >
                    Login to Wordfolio
                </Typography>
                <Box
                    className={styles.form}
                    component="form"
                    onSubmit={handleSubmit(onSubmit)}
                    noValidate
                >
                    {errors.root && (
                        <Alert severity="error" className={styles.alert}>
                            {errors.root.message}
                        </Alert>
                    )}

                    <TextField
                        fullWidth
                        margin="normal"
                        id="email"
                        label="Email"
                        type="email"
                        autoComplete="email"
                        error={!!errors.email}
                        helperText={errors.email?.message}
                        {...register("email")}
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
                        {...register("password")}
                    />

                    <Button
                        fullWidth
                        type="submit"
                        variant="contained"
                        disabled={loginMutation.isPending}
                        className={styles.button}
                    >
                        {loginMutation.isPending ? "Logging in..." : "Login"}
                    </Button>

                    <Box className={styles.footer}>
                        <Typography variant="body2">
                            Don't have an account?{" "}
                            <MuiLink
                                component={Link}
                                {...registerPath()}
                                underline="hover"
                            >
                                Register here
                            </MuiLink>
                        </Typography>
                    </Box>
                </Box>
            </Container>
        </div>
    );
};
