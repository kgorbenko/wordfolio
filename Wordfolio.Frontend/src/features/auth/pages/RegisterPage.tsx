import { useCallback } from "react";
import { useNavigate, Link } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
    Alert,
    Button,
    Link as MuiLink,
    TextField,
    Typography,
} from "@mui/material";

import { useAuthStore } from "../../../shared/stores/authStore";
import { useRegisterMutation } from "../../../shared/api/mutations/auth";
import { usePasswordRequirementsQuery } from "../../../shared/api/queries/auth";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { PasswordField } from "../../../shared/components/PasswordField";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { SignalApertureAuthBackground } from "../components/SignalApertureAuthBackground";
import { SignalApertureDialogPaper } from "../components/SignalApertureDialogPaper";
import { parseApiError } from "../errorHandling";
import { loginPath, homePath, registerRouteApi } from "../routes";
import { getSafeRedirectPath } from "../../../shared/utils/redirectUtils";
import {
    createRegisterSchema,
    type RegisterFormData,
} from "../schemas/authSchemas";

import styles from "../components/SignalApertureAuthView.module.scss";

export const RegisterPage = () => {
    const navigate = useNavigate();
    const search = registerRouteApi.useSearch();
    const safeRedirect = getSafeRedirectPath(search.redirect);
    const loginNavigation = safeRedirect
        ? loginPath({ redirect: safeRedirect })
        : loginPath();
    const setTokens = useAuthStore((state) => state.setTokens);
    const {
        data: passwordRequirements,
        isLoading: isLoadingRequirements,
        isError: isRequirementsError,
        refetch: refetchPasswordRequirements,
    } = usePasswordRequirementsQuery();

    const {
        register,
        handleSubmit,
        setError,
        formState: { errors },
    } = useForm<RegisterFormData>({
        resolver: passwordRequirements
            ? zodResolver(createRegisterSchema(passwordRequirements))
            : undefined,
    });

    const registerMutation = useRegisterMutation({
        onSuccess: async (data) => {
            setTokens(data);
            const destination = getSafeRedirectPath(
                safeRedirect,
                homePath().to
            );
            await navigate({ to: destination, replace: true });
        },
        onError: (error) => {
            const errorMessages = parseApiError(error);

            const errorMessage =
                errorMessages.length > 0
                    ? errorMessages.join("\n")
                    : "An error occurred during registration. Please try again.";

            setError("root", {
                type: "server",
                message: errorMessage,
            });
        },
    });

    const onSubmit = useCallback(
        (data: RegisterFormData) => {
            registerMutation.mutate({
                email: data.email,
                password: data.password,
            });
        },
        [registerMutation]
    );

    const renderContent = () => {
        if (isLoadingRequirements) {
            return (
                <div className={styles.stateBlock}>
                    <ContentSkeleton variant="form" />
                </div>
            );
        }

        if (isRequirementsError || !passwordRequirements) {
            return (
                <div className={styles.stateBlock}>
                    <RetryOnError
                        title="Failed to Load Password Requirements"
                        description="Something went wrong while loading password requirements. Please try again."
                        onRetry={() => void refetchPasswordRequirements()}
                    />
                </div>
            );
        }

        return (
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
                    disabled={registerMutation.isPending}
                    error={!!errors.email}
                    helperText={errors.email?.message}
                    {...register("email")}
                />

                <PasswordField
                    fullWidth
                    id="password"
                    label="Password"
                    autoComplete="new-password"
                    disabled={registerMutation.isPending}
                    error={!!errors.password}
                    helperText={errors.password?.message}
                    {...register("password")}
                />

                <PasswordField
                    fullWidth
                    id="confirmPassword"
                    label="Confirm Password"
                    autoComplete="new-password"
                    disabled={registerMutation.isPending}
                    error={!!errors.confirmPassword}
                    helperText={errors.confirmPassword?.message}
                    {...register("confirmPassword")}
                />

                <Button
                    variant="contained"
                    color="primary"
                    fullWidth
                    type="submit"
                    disabled={registerMutation.isPending}
                    aria-busy={registerMutation.isPending}
                >
                    {registerMutation.isPending
                        ? "Opening archive…"
                        : "Create archive"}
                </Button>
            </form>
        );
    };

    return (
        <SignalApertureAuthBackground>
            <div className={styles.shell}>
                <SignalApertureDialogPaper
                    title="Wordfolio"
                    subtitle="open a new archive"
                    footer={
                        <Typography variant="body2">
                            Already have an account?{" "}
                            <MuiLink
                                component={Link}
                                {...loginNavigation}
                                underline="hover"
                            >
                                Login here
                            </MuiLink>
                        </Typography>
                    }
                >
                    {renderContent()}
                </SignalApertureDialogPaper>
            </div>
        </SignalApertureAuthBackground>
    );
};
