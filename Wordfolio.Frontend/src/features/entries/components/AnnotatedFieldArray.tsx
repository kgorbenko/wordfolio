import {
    Control,
    FieldErrors,
    UseFormRegister,
    useFieldArray,
    FieldPath,
    FieldArrayPath,
} from "react-hook-form";
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
import FormatQuoteIcon from "@mui/icons-material/FormatQuote";

import { EntryFormInput } from "../schemas/entrySchemas";
import { AnnotatedItemColor } from "../types";
import styles from "./AnnotatedFieldArray.module.scss";

type DefinitionTextPath = `definitions.${number}.definitionText`;
type TranslationTextPath = `translations.${number}.translationText`;
type ExamplesPath =
    | `definitions.${number}.examples`
    | `translations.${number}.examples`;
type ExampleTextPath =
    | `definitions.${number}.examples.${number}.exampleText`
    | `translations.${number}.examples.${number}.exampleText`;

interface AnnotatedFieldArrayProps {
    readonly control: Control<EntryFormInput>;
    readonly register: UseFormRegister<EntryFormInput>;
    readonly errors: FieldErrors<EntryFormInput>;
    readonly index: number;
    readonly color: AnnotatedItemColor;
    readonly textFieldPath: DefinitionTextPath | TranslationTextPath;
    readonly examplesPath: ExamplesPath;
    readonly getExampleTextPath: (exIndex: number) => ExampleTextPath;
    readonly getTextError: () => string | undefined;
    readonly getExampleError: (exIndex: number) => string | undefined;
    readonly onRemove: () => void;
    readonly isLoading?: boolean;
}

export const AnnotatedFieldArray = ({
    control,
    register,
    index,
    color,
    textFieldPath,
    examplesPath,
    getExampleTextPath,
    getTextError,
    getExampleError,
    onRemove,
    isLoading = false,
}: AnnotatedFieldArrayProps) => {
    const theme = useTheme();

    const {
        fields: exampleFields,
        append: appendExample,
        remove: removeExample,
    } = useFieldArray({
        control,
        name: examplesPath as FieldArrayPath<EntryFormInput>,
    });

    const handleAddExample = () => {
        appendExample(
            { id: `ex-${Date.now()}`, exampleText: "", source: "Custom" },
            { shouldFocus: true }
        );
    };

    const paletteColor =
        color === "primary"
            ? theme.palette.primary.main
            : theme.palette.secondary.main;
    const borderColor = alpha(paletteColor, 0.2);
    const exampleBorderColor = alpha(paletteColor, 0.3);
    const textError = getTextError();

    return (
        <Paper
            variant="outlined"
            className={styles.card}
            sx={{ borderColor }}
            data-testid="card"
        >
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
                            size="small"
                            disabled={isLoading}
                            error={!!textError}
                            helperText={textError}
                            {...register(
                                textFieldPath as FieldPath<EntryFormInput>
                            )}
                            data-testid="text-field"
                        />
                        <IconButton
                            size="small"
                            onClick={onRemove}
                            sx={{ color: "error.main" }}
                            disabled={isLoading}
                            data-testid="delete-button"
                        >
                            <DeleteIcon fontSize="small" />
                        </IconButton>
                    </Box>

                    {exampleFields.length > 0 && (
                        <Box className={styles.examplesContainer}>
                            {exampleFields.map((exampleField, exIndex) => {
                                const exampleError = getExampleError(exIndex);
                                return (
                                    <Box
                                        key={exampleField.id}
                                        className={styles.example}
                                        sx={{
                                            borderLeft: `2px solid ${exampleBorderColor}`,
                                        }}
                                        data-testid="example"
                                    >
                                        <FormatQuoteIcon
                                            className={styles.quoteIcon}
                                        />
                                        <TextField
                                            fullWidth
                                            multiline
                                            size="small"
                                            disabled={isLoading}
                                            error={!!exampleError}
                                            helperText={exampleError}
                                            placeholder="Example sentence"
                                            className={styles.exampleTextField}
                                            {...register(
                                                getExampleTextPath(
                                                    exIndex
                                                ) as FieldPath<EntryFormInput>
                                            )}
                                            data-testid="example-text-field"
                                        />
                                        <IconButton
                                            size="small"
                                            onClick={() =>
                                                removeExample(exIndex)
                                            }
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
                    )}

                    <Button
                        size="small"
                        startIcon={<AddIcon />}
                        onClick={handleAddExample}
                        className={styles.addButton}
                        disabled={isLoading}
                        data-testid="add-example-button"
                    >
                        Add Example
                    </Button>
                </Box>
            </Box>
        </Paper>
    );
};
