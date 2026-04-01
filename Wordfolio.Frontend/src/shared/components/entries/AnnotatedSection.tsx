import { Box, Typography } from "@mui/material";

import type { AnnotatedItemColor } from "../../api/types/entries";
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
        <Box
            className={styles.header}
            sx={{
                "&::before": {
                    backgroundColor: `${color}.main`,
                },
            }}
        >
            <Typography variant="overline" className={styles.label}>
                {title}
            </Typography>
        </Box>
        <Box className={styles.items}>{children}</Box>
    </Box>
);
