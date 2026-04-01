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
    TextField,
    IconButton,
    InputAdornment,
    Button,
} from "@mui/material";
import CloseIcon from "@mui/icons-material/Close";
import AddIcon from "@mui/icons-material/Add";

import type { EntryFormInput } from "../../schemas/entryFormSchemas";
import { ExampleSource } from "../../api/types/entries";
import type { AnnotatedItemColor } from "../../api/types/entries";
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
    readonly textFieldPath: DefinitionTextPath | TranslationTextPath;
    readonly examplesPath: ExamplesPath;
    readonly getExampleTextPath: (exIndex: number) => ExampleTextPath;
    readonly getTextError: () => string | undefined;
    readonly getExampleError: (exIndex: number) => string | undefined;
    readonly onRemove: () => void;
    readonly color: AnnotatedItemColor;
    readonly isLoading?: boolean;
}

export const AnnotatedFieldArray = ({
    control,
    register,
    textFieldPath,
    examplesPath,
    getExampleTextPath,
    getTextError,
    getExampleError,
    onRemove,
    color,
    isLoading = false,
}: AnnotatedFieldArrayProps) => {
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
            {
                id: `ex-${Date.now()}`,
                exampleText: "",
                source: ExampleSource.Custom,
            },
            { shouldFocus: true }
        );
    };

    const textError = getTextError();

    return (
        <Box className={styles.item} data-testid="card">
            <TextField
                fullWidth
                multiline
                minRows={1}
                size="small"
                disabled={isLoading}
                error={!!textError}
                helperText={textError}
                {...register(textFieldPath as FieldPath<EntryFormInput>)}
                slotProps={{
                    input: {
                        endAdornment: (
                            <InputAdornment position="end">
                                <IconButton
                                    onClick={onRemove}
                                    disabled={isLoading}
                                    edge="end"
                                    data-testid="delete-button"
                                >
                                    <CloseIcon />
                                </IconButton>
                            </InputAdornment>
                        ),
                    },
                }}
                data-testid="text-field"
            />

            {exampleFields.length > 0 && (
                <Box className={styles.examplesContainer}>
                    {exampleFields.map((exampleField, exIndex) => {
                        const exampleError = getExampleError(exIndex);
                        return (
                            <TextField
                                key={exampleField.id}
                                fullWidth
                                multiline
                                minRows={1}
                                size="small"
                                disabled={isLoading}
                                error={!!exampleError}
                                helperText={exampleError}
                                placeholder="e.g. Example sentence"
                                {...register(
                                    getExampleTextPath(
                                        exIndex
                                    ) as FieldPath<EntryFormInput>
                                )}
                                slotProps={{
                                    input: {
                                        endAdornment: (
                                            <InputAdornment position="end">
                                                <IconButton
                                                    onClick={() =>
                                                        removeExample(exIndex)
                                                    }
                                                    disabled={isLoading}
                                                    edge="end"
                                                    data-testid="example-delete-button"
                                                >
                                                    <CloseIcon />
                                                </IconButton>
                                            </InputAdornment>
                                        ),
                                    },
                                }}
                                data-testid="example-text-field"
                            />
                        );
                    })}
                </Box>
            )}

            <Button
                size="small"
                color={color}
                startIcon={<AddIcon />}
                onClick={handleAddExample}
                className={styles.addExampleButton}
                disabled={isLoading}
                data-testid="add-example-button"
            >
                Add Example
            </Button>
        </Box>
    );
};
