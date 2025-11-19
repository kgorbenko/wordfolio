import { useNavigate, Link, useSearch } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";

import { ApiError } from "../api/authApi";
import { useAuthStore } from "../stores/authStore";
import { useLoginMutation } from "../mutations/useLoginMutation";
import { createLoginSchema, LoginFormData } from "../schemas/authSchemas";
import { useNotificationContext } from "../contexts/NotificationContext";
import {
    Container,
    Typography,
    TextField,
    Button,
    Box,
    Alert,
    Link as MuiLink,
} from "@mui/material";

import "./LoginPage.css";

export const LoginPage = () => {
    const navigate = useNavigate();
    const search = useSearch({ from: "/login" });
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
            navigate({ to: "/" });
        },
        onError: (error: ApiError) => {
            if (error.status === 401) {
                setError("root", {
                    type: "manual",
                    message: "Invalid email or password",
                });
            } else {
                setError("root", {
                    type: "manual",
                    message: "An error occurred. Please try again.",
                });
            }
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
            <Container maxWidth="sm" className="login-container">
                <Typography
                    className="page-header"
                    variant="h5"
                    component="h1"
                    gutterBottom
                    align="center"
                >
                    Login to Wordfolio
                </Typography>
                <Box
                    className="login-form"
                    component="form"
                    onSubmit={handleSubmit(onSubmit)}
                    noValidate
                >
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

                    {errors.root && (
                        <Alert severity="error" className="login-alert">
                            {errors.root.message}
                        </Alert>
                    )}

                    <Button
                        fullWidth
                        type="submit"
                        variant="contained"
                        disabled={loginMutation.isPending}
                        className="login-button"
                    >
                        {loginMutation.isPending ? "Logging in..." : "Login"}
                    </Button>

                    <Box className="login-footer">
                        <Typography variant="body2">
                            Don't have an account?{" "}
                            <MuiLink
                                component={Link}
                                to="/register"
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
