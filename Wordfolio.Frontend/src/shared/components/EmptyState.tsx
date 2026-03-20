import { ReactNode } from "react";
import { Box, Typography } from "@mui/material";

import styles from "./EmptyState.module.scss";

interface EmptyStateProps {
    readonly icon?: ReactNode;
    readonly title?: string;
    readonly description?: string;
}

const DefaultIcon = () => (
    <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
        <rect
            x="6"
            y="8"
            width="20"
            height="16"
            rx="2"
            stroke="currentColor"
            strokeWidth="1.5"
        />
        <path d="M6 14h20" stroke="currentColor" strokeWidth="1.5" />
        <circle cx="16" cy="21" r="1.5" fill="currentColor" />
    </svg>
);

export const EmptyState = ({
    icon = <DefaultIcon />,
    title = "Nothing here yet",
    description = "Add your first item to get started",
}: EmptyStateProps) => (
    <Box className={styles.emptyState} sx={{ color: "divider" }}>
        {icon}
        <Typography
            variant="body1"
            color="text.primary"
            className={styles.title}
        >
            {title}
        </Typography>
        <Typography
            variant="body2"
            color="text.secondary"
            className={styles.description}
        >
            {description}
        </Typography>
    </Box>
);
