import { Box, Typography, IconButton, alpha, useTheme } from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import { ExampleItem, AnnotatedItemColor } from "../types";
import styles from "./ExamplesList.module.scss";

interface ExamplesListProps {
    readonly examples: ExampleItem[];
    readonly color: AnnotatedItemColor;
    readonly onDelete: (index: number) => void;
    readonly isLoading?: boolean;
}

export const ExamplesList = ({
    examples,
    color,
    onDelete,
    isLoading = false,
}: ExamplesListProps) => {
    const theme = useTheme();

    if (examples.length === 0) {
        return null;
    }

    const borderColor =
        color === "primary"
            ? alpha(theme.palette.primary.main, 0.3)
            : alpha(theme.palette.secondary.main, 0.3);

    return (
        <Box className={styles.container}>
            {examples.map((example, index) => (
                <Box
                    key={example.id}
                    className={styles.example}
                    sx={{ borderLeft: `2px solid ${borderColor}` }}
                >
                    <FormatQuoteIcon className={styles.quoteIcon} />
                    <Typography
                        variant="body2"
                        color="text.secondary"
                        className={styles.text}
                    >
                        {example.exampleText}
                    </Typography>
                    <IconButton
                        size="small"
                        onClick={() => onDelete(index)}
                        sx={{ color: "error.main" }}
                        disabled={isLoading}
                    >
                        <DeleteIcon fontSize="small" />
                    </IconButton>
                </Box>
            ))}
        </Box>
    );
};
