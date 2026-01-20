import {
    Box,
    Paper,
    Chip,
    TextField,
    IconButton,
    Button,
    alpha,
    useTheme,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import AddIcon from "@mui/icons-material/Add";

import { ExampleItem, AnnotatedItemColor } from "../types";
import { EditableExamplesList } from "./EditableExamplesList";
import styles from "./AnnotatedItemCard.module.scss";

interface ExampleError {
    exampleText?: { message?: string };
}

interface AnnotatedItemCardProps {
    readonly index: number;
    readonly color: AnnotatedItemColor;
    readonly textValue: string;
    readonly error?: string;
    readonly examples: ExampleItem[];
    readonly exampleErrors?: (ExampleError | undefined)[];
    readonly autoFocus?: boolean;
    readonly autoFocusExampleIndex?: number;
    readonly isLoading?: boolean;
    readonly onTextChange: (value: string) => void;
    readonly onDelete: () => void;
    readonly onAddExample: () => void;
    readonly onExampleTextChange: (exampleIndex: number, value: string) => void;
    readonly onDeleteExample: (exampleIndex: number) => void;
}

export const AnnotatedItemCard = ({
    index,
    color,
    textValue,
    error,
    examples,
    exampleErrors,
    autoFocus = false,
    autoFocusExampleIndex,
    isLoading = false,
    onTextChange,
    onDelete,
    onAddExample,
    onExampleTextChange,
    onDeleteExample,
}: AnnotatedItemCardProps) => {
    const theme = useTheme();

    const borderColor =
        color === "primary"
            ? alpha(theme.palette.primary.main, 0.2)
            : alpha(theme.palette.secondary.main, 0.2);

    return (
        <Paper variant="outlined" className={styles.card} sx={{ borderColor }}>
            <Box className={styles.content}>
                <Chip
                    label={index + 1}
                    size="small"
                    color={color}
                    className={styles.chip}
                />
                <Box className={styles.body}>
                    <Box className={styles.textRow}>
                        <TextField
                            fullWidth
                            multiline
                            value={textValue}
                            onChange={(e) => onTextChange(e.target.value)}
                            size="small"
                            autoFocus={autoFocus}
                            disabled={isLoading}
                            error={!!error}
                            helperText={error}
                        />
                        <IconButton
                            size="small"
                            onClick={onDelete}
                            sx={{ color: "error.main" }}
                            disabled={isLoading}
                        >
                            <DeleteIcon fontSize="small" />
                        </IconButton>
                    </Box>
                    <EditableExamplesList
                        examples={examples}
                        errors={exampleErrors}
                        color={color}
                        autoFocusIndex={autoFocusExampleIndex}
                        isLoading={isLoading}
                        onTextChange={onExampleTextChange}
                        onDelete={onDeleteExample}
                    />
                    <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={onAddExample}
                        className={styles.addButton}
                        disabled={isLoading}
                    >
                        Add Example
                    </Button>
                </Box>
            </Box>
        </Paper>
    );
};
