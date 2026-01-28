import { Box, Typography, alpha, useTheme } from "@mui/material";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import { Example } from "../types";
import { AnnotatedItemColor } from "../types";
import styles from "./ExampleDisplay.module.scss";

interface ExampleDisplayProps {
    readonly example: Example;
    readonly color: AnnotatedItemColor;
}

export const ExampleDisplay = ({ example, color }: ExampleDisplayProps) => {
    const theme = useTheme();
    const paletteColor =
        color === "primary" ? theme.palette.primary : theme.palette.secondary;

    return (
        <Box
            className={styles.example}
            sx={{ borderLeft: `2px solid ${alpha(paletteColor.main, 0.3)}` }}
        >
            <FormatQuoteIcon
                className={styles.icon}
                sx={{ color: "text.secondary" }}
            />
            <Typography
                variant="body2"
                color="text.secondary"
                className={styles.text}
            >
                {example.exampleText}
            </Typography>
        </Box>
    );
};
