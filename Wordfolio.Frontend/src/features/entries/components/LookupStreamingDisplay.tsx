import { Paper, Box } from "@mui/material";

import styles from "./LookupStreamingDisplay.module.scss";

interface LookupStreamingDisplayProps {
    readonly text: string;
}

export const LookupStreamingDisplay = ({
    text,
}: LookupStreamingDisplayProps) => (
    <Paper variant="outlined" className={styles.paper}>
        {text}
        <Box component="span" className={styles.cursor} />
    </Paper>
);
