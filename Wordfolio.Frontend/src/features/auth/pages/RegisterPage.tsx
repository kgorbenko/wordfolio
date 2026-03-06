import { useCallback } from "react";
import { useNavigate, Link } from "@tanstack/react-router";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
    Container,
    Typography,
    TextField,
    Button,
    Box,
    Alert,
    Link as MuiLink,
} from "@mui/material";

import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { useRegisterMutation } from "../hooks/useRegisterMutation";
import { usePasswordRequirementsQuery } from "../hooks/usePasswordRequirementsQuery";
import { createRegisterSchema, RegisterFormData } from "../schemas/authSchemas";
import { parseApiError } from "../errorHandling";
import { loginPath } from "../routes";

import styles from "./RegisterPage.module.scss";

export const RegisterPage = () => {
    const navigate = useNavigate();
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
        onSuccess: () => {
            navigate(loginPath());
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

    const renderContent = useCallback(() => {
        if (isLoadingRequirements) return <ContentSkeleton variant="form" />;

        if (isRequirementsError || !passwordRequirements) {
            return (
                <RetryOnError
                    onRetry={() => void refetchPasswordRequirements()}
                />
            );
        }

        return (
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
                    autoComplete="new-password"
                    error={!!errors.password}
                    helperText={errors.password?.message}
                    {...register("password")}
                />

                <TextField
                    fullWidth
                    margin="normal"
                    id="confirmPassword"
                    label="Confirm Password"
                    type="password"
                    autoComplete="new-password"
                    error={!!errors.confirmPassword}
                    helperText={errors.confirmPassword?.message}
                    {...register("confirmPassword")}
                />

                <Button
                    fullWidth
                    type="submit"
                    variant="contained"
                    disabled={registerMutation.isPending}
                    className={styles.button}
                >
                    {registerMutation.isPending ? "Registering..." : "Register"}
                </Button>

                <Box className={styles.footer}>
                    <Typography variant="body2">
                        Already have an account?{" "}
                        <MuiLink
                            component={Link}
                            {...loginPath()}
                            underline="hover"
                        >
                            Login here
                        </MuiLink>
                    </Typography>
                </Box>
            </Box>
        );
    }, [
        isLoadingRequirements,
        isRequirementsError,
        passwordRequirements,
        refetchPasswordRequirements,
        handleSubmit,
        onSubmit,
        errors.root,
        errors.email,
        errors.password,
        errors.confirmPassword,
        register,
        registerMutation.isPending,
    ]);

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
                    Sign Up for Wordfolio
                </Typography>
                {renderContent()}
            </Container>
        </div>
    );
};
