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

import { useAuthStore } from "../../../shared/stores/authStore";
import { PasswordField } from "../../../shared/components/PasswordField";
import { useLoginMutation } from "../hooks/useLoginMutation";
import { createLoginSchema, LoginFormData } from "../schemas/authSchemas";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
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
        setValue,
        formState: { errors },
    } = useForm<LoginFormData>({
        resolver: zodResolver(createLoginSchema()),
    });

    const loginMutation = useLoginMutation({
        onSuccess: async (data) => {
            setTokens(data);
            await navigate(homePath());
        },
        onError: (error) => {
            const errorMessage =
                error.status === 401
                    ? "Invalid email or password"
                    : "An error occurred. Please try again.";

            setError("root", {
                type: "server",
                message: errorMessage,
            });

            setValue("password", "");
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
                        id="email"
                        label="Email"
                        type="email"
                        autoComplete="email"
                        disabled={loginMutation.isPending}
                        error={!!errors.email}
                        helperText={errors.email?.message}
                        {...register("email")}
                    />

                    <PasswordField
                        fullWidth
                        id="password"
                        label="Password"
                        autoComplete="current-password"
                        disabled={loginMutation.isPending}
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
