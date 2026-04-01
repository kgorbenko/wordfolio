import { Box, Typography } from "@mui/material";

import type { Example } from "../../api/types/entries";
import { ExampleDisplay } from "./ExampleDisplay";
import styles from "./AnnotatedItemCard.module.scss";

interface AnnotatedItemCardProps {
    readonly text: string;
    readonly examples: Example[];
}

export const AnnotatedItemCard = ({
    text,
    examples,
}: AnnotatedItemCardProps) => (
    <Box className={styles.item}>
        <Typography variant="body1" className={styles.text}>
            {text}
        </Typography>
        {examples.length > 0 && (
            <Box className={styles.examples}>
                {examples.map((ex) => (
                    <ExampleDisplay key={ex.id} example={ex} />
                ))}
            </Box>
        )}
    </Box>
);
