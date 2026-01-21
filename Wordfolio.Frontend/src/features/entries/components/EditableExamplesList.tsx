import { Box, TextField, IconButton, alpha, useTheme } from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import { ExampleItem, AnnotatedItemColor } from "../types";
import styles from "./EditableExamplesList.module.scss";

interface ExampleError {
    exampleText?: { message?: string };
}

interface EditableExamplesListProps {
    readonly examples: ExampleItem[];
    readonly errors?: (ExampleError | undefined)[];
    readonly color: AnnotatedItemColor;
    readonly autoFocusIndex?: number;
    readonly isLoading?: boolean;
    readonly onTextChange: (index: number, value: string) => void;
    readonly onDelete: (index: number) => void;
}

export const EditableExamplesList = ({
    examples,
    errors,
    color,
    autoFocusIndex,
    isLoading = false,
    onTextChange,
    onDelete,
}: EditableExamplesListProps) => {
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
            {examples.map((example, index) => {
                const error = errors?.[index]?.exampleText?.message;
                return (
                    <Box
                        key={example.id}
                        className={styles.example}
                        sx={{ borderLeft: `2px solid ${borderColor}` }}
                        data-testid="example"
                    >
                        <FormatQuoteIcon className={styles.quoteIcon} />
                        <TextField
                            fullWidth
                            multiline
                            size="small"
                            value={example.exampleText}
                            onChange={(e) =>
                                onTextChange(index, e.target.value)
                            }
                            autoFocus={autoFocusIndex === index}
                            disabled={isLoading}
                            error={!!error}
                            helperText={error}
                            placeholder="Example sentence"
                            className={styles.textField}
                            data-testid="example-text-field"
                        />
                        <IconButton
                            size="small"
                            onClick={() => onDelete(index)}
                            sx={{ color: "error.main" }}
                            disabled={isLoading}
                            data-testid="example-delete-button"
                        >
                            <DeleteIcon fontSize="small" />
                        </IconButton>
                    </Box>
                );
            })}
        </Box>
    );
};
