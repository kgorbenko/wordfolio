import { Box, Divider, Typography } from "@mui/material";

import styles from "./EntryFooter.module.scss";

interface EntryFooterProps {
    readonly createdAt: Date;
    readonly updatedAt: Date;
}

export const EntryFooter = ({ createdAt, updatedAt }: EntryFooterProps) => (
    <>
        <Divider className={styles.divider} />
        <Box className={styles.footer}>
            <Typography variant="body2" color="text.secondary">
                Added {createdAt.toLocaleDateString()} · Updated{" "}
                {updatedAt.toLocaleDateString()}
            </Typography>
        </Box>
    </>
);
