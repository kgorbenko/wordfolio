import { Box, Typography } from "@mui/material";

import type { Example } from "../../api/types/entries";
import styles from "./ExampleDisplay.module.scss";

interface ExampleDisplayProps {
    readonly example: Example;
}

export const ExampleDisplay = ({ example }: ExampleDisplayProps) => (
    <Box className={styles.example}>
        <Typography variant="body2" component="span" className={styles.prefix}>
            e.g.
        </Typography>
        <Typography variant="body2" className={styles.text}>
            {example.exampleText}
        </Typography>
    </Box>
);
