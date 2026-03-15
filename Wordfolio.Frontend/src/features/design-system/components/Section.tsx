import { ReactNode } from "react";
import { Box, Divider, Typography } from "@mui/material";

import styles from "./Section.module.scss";

interface SectionProps {
    readonly title: string;
    readonly children: ReactNode;
}

export const Section = ({ title, children }: SectionProps) => (
    <Box className={styles.section}>
        <Typography variant="h2" className={styles.sectionTitle}>
            {title}
        </Typography>
        <Divider className={styles.sectionDivider} />
        {children}
    </Box>
);
