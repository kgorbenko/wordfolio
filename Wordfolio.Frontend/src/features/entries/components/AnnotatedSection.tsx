import { Box, Typography } from "@mui/material";

import { AnnotatedItemColor } from "../types";
import styles from "./AnnotatedSection.module.scss";

interface AnnotatedSectionProps {
    readonly title: string;
    readonly color: AnnotatedItemColor;
    readonly children: React.ReactNode;
}

export const AnnotatedSection = ({
    title,
    color,
    children,
}: AnnotatedSectionProps) => (
    <Box className={styles.section}>
        <Typography
            variant="h6"
            fontWeight={600}
            sx={{ color: `${color}.main` }}
            className={styles.title}
        >
            {title}
        </Typography>
        <Box className={styles.items}>{children}</Box>
    </Box>
);
