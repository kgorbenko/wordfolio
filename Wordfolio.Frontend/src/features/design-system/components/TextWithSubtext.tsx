import { Box, Typography } from "@mui/material";

import styles from "./TextWithSubtext.module.scss";

interface TextWithSubtextProps {
    readonly text: string;
    readonly subtext: string;
}

export const TextWithSubtext = ({ text, subtext }: TextWithSubtextProps) => (
    <Box className={styles.container}>
        <Typography variant="body1" color="text.primary" noWrap className={styles.text}>
            {text}
        </Typography>
        <Typography variant="body2" color="text.accent" noWrap className={styles.subtext}>
            {subtext}
        </Typography>
    </Box>
);
