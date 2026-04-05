import { Box } from "@mui/material";

import styles from "./WordfolioBrand.module.scss";

export const WordfolioBrand = () => (
    <>
        <Box
            className={styles.logo}
            sx={{
                bgcolor: "rgba(0, 0, 0, 0.15)",
                border: "1px solid rgba(0, 0, 0, 0.20)",
            }}
        />
        <Box
            component="span"
            className={styles.wordmark}
            sx={{ color: "text.topbarPrimary" }}
        >
            Wordfolio
        </Box>
    </>
);
