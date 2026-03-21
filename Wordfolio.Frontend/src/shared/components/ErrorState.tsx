import { Box, Button, Typography } from "@mui/material";

import styles from "./ErrorState.module.scss";

interface ErrorStateProps {
    readonly title?: string;
    readonly description?: string;
    readonly onRetry?: () => void;
}

const ErrorIcon = () => (
    <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
        <circle
            cx="16"
            cy="16"
            r="12"
            stroke="currentColor"
            strokeWidth="1.5"
        />
        <path
            d="M16 10v8"
            stroke="currentColor"
            strokeWidth="1.5"
            strokeLinecap="round"
        />
        <circle cx="16" cy="22" r="1" fill="currentColor" />
    </svg>
);

export const ErrorState = ({
    title = "Something went wrong",
    description = "An error occurred while loading this content",
    onRetry,
}: ErrorStateProps) => (
    <Box className={styles.errorState} sx={{ color: "error.main" }}>
        <ErrorIcon />
        <Typography variant="body1" color="text.primary">
            {title}
        </Typography>
        <Typography variant="body2" color="text.secondary">
            {description}
        </Typography>
        {onRetry && (
            <Button
                variant="outlined"
                color="error"
                onClick={onRetry}
                className={styles.retryButton}
            >
                Retry
            </Button>
        )}
    </Box>
);
