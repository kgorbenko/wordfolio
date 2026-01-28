import { Box, Paper, Chip, Typography, alpha, useTheme } from "@mui/material";

import { Example, AnnotatedItemColor } from "../types";
import { ExampleDisplay } from "./ExampleDisplay";
import styles from "./AnnotatedItemCard.module.scss";

interface AnnotatedItemCardProps {
    readonly index: number;
    readonly text: string;
    readonly examples: Example[];
    readonly color: AnnotatedItemColor;
}

export const AnnotatedItemCard = ({
    index,
    text,
    examples,
    color,
}: AnnotatedItemCardProps) => {
    const theme = useTheme();
    const paletteColor =
        color === "primary" ? theme.palette.primary : theme.palette.secondary;

    return (
        <Paper
            variant="outlined"
            className={styles.card}
            sx={{ borderColor: alpha(paletteColor.main, 0.2) }}
        >
            <Box className={styles.content}>
                <Chip
                    label={index + 1}
                    size="small"
                    color={color}
                    className={styles.chip}
                />
                <Box className={styles.body}>
                    <Typography variant="body1" className={styles.text}>
                        {text}
                    </Typography>
                    {examples.length > 0 && (
                        <Box className={styles.examples}>
                            {examples.map((ex) => (
                                <ExampleDisplay
                                    key={ex.id}
                                    example={ex}
                                    color={color}
                                />
                            ))}
                        </Box>
                    )}
                </Box>
            </Box>
        </Paper>
    );
};
