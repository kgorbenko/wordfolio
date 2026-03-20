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
            <Typography variant="h1" className={styles.title}>
                {title}
            </Typography>
            {actions}
        </Box>
        <Typography
            variant="body1"
            color="text.secondary"
            className={styles.description}
            sx={{ visibility: description ? "visible" : "hidden" }}
        >
            {description ?? "\u00a0"}
        </Typography>
    </Box>
);
