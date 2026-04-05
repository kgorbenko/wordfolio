import { ReactNode } from "react";
import { Box, Typography } from "@mui/material";
import DataArrayIcon from "@mui/icons-material/DataArray";

import styles from "./EmptyState.module.scss";

interface EmptyStateProps {
    readonly icon?: ReactNode;
    readonly title?: string;
    readonly description?: string;
}

const DefaultIcon = () => (
    <DataArrayIcon sx={{ fontSize: 32, color: "primary.main" }} />
);

export const EmptyState = ({
    icon = <DefaultIcon />,
    title = "Nothing here yet",
    description = "Add your first item to get started",
}: EmptyStateProps) => (
    <Box className={styles.emptyState}>
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
