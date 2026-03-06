import { Box, Typography, Button, alpha, useTheme } from "@mui/material";
import { ReactNode } from "react";

import "./EmptyState.scss";

interface EmptyStateProps {
    readonly icon: ReactNode;
    readonly title: string;
    readonly description: string;
    readonly actionLabel?: string;
    readonly onAction?: () => void;
}

export const EmptyState = ({
    icon,
    title,
    description,
    actionLabel,
    onAction,
}: EmptyStateProps) => {
    const theme = useTheme();

    return (
        <Box className="empty-state">
            <Box
                className="icon-wrapper"
                sx={{ bgcolor: alpha(theme.palette.primary.main, 0.1) }}
            >
                {icon}
            </Box>
            <Typography variant="h5" gutterBottom fontWeight={600}>
                {title}
            </Typography>
            <Typography
                className="description"
                variant="body1"
                color="text.secondary"
            >
                {description}
            </Typography>
            {actionLabel && onAction && (
                <Button variant="contained" size="large" onClick={onAction}>
                    {actionLabel}
                </Button>
            )}
        </Box>
    );
};
