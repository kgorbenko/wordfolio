import { Box, Typography } from "@mui/material";
import styles from "./PageHeader.module.scss";

interface PageHeaderProps {
    readonly title: string;
    readonly description?: string;
    readonly actions?: React.ReactNode;
}

export const PageHeader = ({
    title,
    description,
    actions,
}: PageHeaderProps) => (
    <Box className={styles.container}>
        <Box className={styles.topRow}>
            <Typography variant="h4" fontWeight={600} className={styles.title}>
                {title}
            </Typography>
            {actions}
        </Box>
        {description && (
            <Typography
                variant="body1"
                color="text.secondary"
                className={styles.description}
            >
                {description}
            </Typography>
        )}
    </Box>
);
