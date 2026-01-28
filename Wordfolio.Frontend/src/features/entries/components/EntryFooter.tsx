import { Box, Divider, Typography } from "@mui/material";

import styles from "./EntryFooter.module.scss";

interface EntryFooterProps {
    readonly createdAt: Date;
    readonly updatedAt: Date | null;
}

export const EntryFooter = ({ createdAt, updatedAt }: EntryFooterProps) => (
    <>
        <Divider sx={{ my: 4 }} />
        <Box className={styles.footer}>
            <Typography variant="caption" color="text.secondary">
                Added {createdAt.toLocaleDateString()}
                {updatedAt && ` · Updated ${updatedAt.toLocaleDateString()}`}
            </Typography>
        </Box>
    </>
);
