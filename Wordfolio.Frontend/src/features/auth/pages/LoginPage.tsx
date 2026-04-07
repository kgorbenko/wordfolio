import { useNavigate, Link } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import {
    Alert,
    Button,
    Link as MuiLink,
    TextField,
    Typography,
} from "@mui/material";

import { useAuthStore } from "../../../shared/stores/authStore";
import { PasswordField } from "../../../shared/components/PasswordField";
import { SignalApertureAuthBackground } from "../components/SignalApertureAuthBackground";
import { SignalApertureDialogPaper } from "../components/SignalApertureDialogPaper";
import { useLoginMutation } from "../hooks/useLoginMutation";
import { createLoginSchema, type LoginFormData } from "../schemas/authSchemas";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { loginRouteApi, homePath, registerPath } from "../routes";

import styles from "../components/SignalApertureAuthView.module.scss";

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
            await navigate({ ...homePath(), replace: true });
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
        <SignalApertureAuthBackground>
            <div className={styles.shell}>
                <SignalApertureDialogPaper
                    title="Wordfolio"
                    subtitle="enter your archive"
                    footer={
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
                    }
                >
                    <form
                        className={styles.form}
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
                            variant="contained"
                            color="primary"
                            fullWidth
                            type="submit"
                            disabled={loginMutation.isPending}
                            aria-busy={loginMutation.isPending}
                        >
                            {loginMutation.isPending
                                ? "Authenticating…"
                                : "Enter archive"}
                        </Button>
                    </form>
                </SignalApertureDialogPaper>
            </div>
        </SignalApertureAuthBackground>
    );
};
