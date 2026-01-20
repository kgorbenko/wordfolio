import { useEffect } from "react";
import { useForm, useFieldArray, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Box, TextField, Button, Typography } from "@mui/material";

import {
    entryFormSchema,
    EntryFormInput,
    EntryFormData,
} from "../schemas/entrySchemas";
import {
    EntryFormValues,
    EntryFormOutput,
    DefinitionItem,
    TranslationItem,
    ExampleItem,
} from "../types";
import {
    DefinitionRequest,
    TranslationRequest,
    ExampleRequest,
} from "../../../api/entriesApi";
import { AnnotatedItemSection } from "./AnnotatedItemSection";
import { AnnotatedItemCard } from "./AnnotatedItemCard";
import styles from "./EntryForm.module.scss";

interface EntryFormProps {
    readonly defaultValues?: EntryFormValues;
    readonly onSubmit: (data: EntryFormOutput) => void;
    readonly onCancel: () => void;
    readonly submitLabel: string;
    readonly isLoading?: boolean;
    readonly showEntryText?: boolean;
    readonly showFooter?: boolean;
    readonly onChange?: (data: EntryFormOutput, isValid: boolean) => void;
}

const mapToOutput = (data: EntryFormData): EntryFormOutput => {
    const definitions: DefinitionRequest[] = data.definitions.map((d) => ({
        definitionText: d.definitionText,
        source: d.source,
        examples: d.examples.map(
            (ex): ExampleRequest => ({
                exampleText: ex.exampleText,
                source: ex.source,
            })
        ),
    }));

    const translations: TranslationRequest[] = data.translations.map((t) => ({
        translationText: t.translationText,
        source: t.source,
        examples: t.examples.map(
            (ex): ExampleRequest => ({
                exampleText: ex.exampleText,
                source: ex.source,
            })
        ),
    }));

    return {
        entryText: data.entryText.trim(),
        definitions,
        translations,
    };
};

const mapToFormInput = (values?: EntryFormValues): EntryFormInput => {
    if (!values) {
        return {
            entryText: "",
            definitions: [],
            translations: [],
        };
    }

    return {
        entryText: values.entryText,
        definitions: values.definitions.map((d) => ({
            id: d.id,
            definitionText: d.definitionText,
            source: d.source,
            examples: d.examples.map((ex) => ({
                id: ex.id,
                exampleText: ex.exampleText,
                source: ex.source,
            })),
        })),
        translations: values.translations.map((t) => ({
            id: t.id,
            translationText: t.translationText,
            source: t.source,
            examples: t.examples.map((ex) => ({
                id: ex.id,
                exampleText: ex.exampleText,
                source: ex.source,
            })),
        })),
    };
};

export const EntryForm = ({
    defaultValues,
    onSubmit,
    onCancel,
    submitLabel,
    isLoading = false,
    showEntryText = true,
    showFooter = true,
    onChange,
}: EntryFormProps) => {
    const {
        register,
        control,
        handleSubmit,
        setValue,
        getValues,
        formState: { errors, isValid },
    } = useForm<EntryFormInput, unknown, EntryFormData>({
        resolver: zodResolver(entryFormSchema),
        defaultValues: mapToFormInput(defaultValues),
        mode: "onChange",
    });

    const {
        fields: definitionFields,
        append: appendDefinition,
        remove: removeDefinition,
        update: updateDefinition,
    } = useFieldArray({
        control,
        name: "definitions",
    });

    const {
        fields: translationFields,
        append: appendTranslation,
        remove: removeTranslation,
        update: updateTranslation,
    } = useFieldArray({
        control,
        name: "translations",
    });

    const watchedDefinitions = useWatch({ control, name: "definitions" });
    const watchedTranslations = useWatch({ control, name: "translations" });

    useEffect(() => {
        if (onChange) {
            const formData = getValues();
            const hasContent =
                formData.definitions.length > 0 ||
                formData.translations.length > 0;
            onChange(mapToOutput(formData as EntryFormData), hasContent);
        }
    }, [watchedDefinitions, watchedTranslations, onChange, getValues]);

    const handleFormSubmit = (data: EntryFormData) => {
        onSubmit(mapToOutput(data));
    };

    const handleAddDefinition = (text: string) => {
        const newDefinition: DefinitionItem = {
            id: `def-${Date.now()}`,
            definitionText: text,
            source: "Manual",
            examples: [],
        };
        appendDefinition(newDefinition);
    };

    const handleAddTranslation = (text: string) => {
        const newTranslation: TranslationItem = {
            id: `trans-${Date.now()}`,
            translationText: text,
            source: "Manual",
            examples: [],
        };
        appendTranslation(newTranslation);
    };

    const handleDefinitionTextChange = (index: number, value: string) => {
        setValue(`definitions.${index}.definitionText`, value, {
            shouldValidate: true,
        });
    };

    const handleTranslationTextChange = (index: number, value: string) => {
        setValue(`translations.${index}.translationText`, value, {
            shouldValidate: true,
        });
    };

    const handleAddDefinitionExample = (defIndex: number, text: string) => {
        const definition = getValues(`definitions.${defIndex}`);
        const newExample: ExampleItem = {
            id: `ex-${Date.now()}`,
            exampleText: text,
            source: "Custom",
        };
        updateDefinition(defIndex, {
            ...definition,
            examples: [...definition.examples, newExample],
        });
    };

    const handleAddTranslationExample = (transIndex: number, text: string) => {
        const translation = getValues(`translations.${transIndex}`);
        const newExample: ExampleItem = {
            id: `ex-${Date.now()}`,
            exampleText: text,
            source: "Custom",
        };
        updateTranslation(transIndex, {
            ...translation,
            examples: [...translation.examples, newExample],
        });
    };

    const handleDeleteDefinitionExample = (
        defIndex: number,
        exampleIndex: number
    ) => {
        const definition = getValues(`definitions.${defIndex}`);
        updateDefinition(defIndex, {
            ...definition,
            examples: definition.examples.filter((_, i) => i !== exampleIndex),
        });
    };

    const handleDeleteTranslationExample = (
        transIndex: number,
        exampleIndex: number
    ) => {
        const translation = getValues(`translations.${transIndex}`);
        updateTranslation(transIndex, {
            ...translation,
            examples: translation.examples.filter((_, i) => i !== exampleIndex),
        });
    };

    const hasContent =
        definitionFields.length > 0 || translationFields.length > 0;

    return (
        <Box
            component="form"
            onSubmit={handleSubmit(handleFormSubmit)}
            noValidate
            className={styles.form}
        >
            {showEntryText && (
                <TextField
                    autoFocus
                    fullWidth
                    label="Word or Phrase"
                    disabled={isLoading}
                    error={!!errors.entryText}
                    helperText={errors.entryText?.message}
                    {...register("entryText")}
                    className={styles.entryTextField}
                />
            )}

            <AnnotatedItemSection
                title="Definitions"
                color="primary"
                emptyMessage="No definitions yet"
                dialogTitle="Add Definition"
                dialogLabel="Definition"
                dialogMultiline
                itemCount={definitionFields.length}
                isLoading={isLoading}
                onAdd={handleAddDefinition}
            >
                {definitionFields.map((field, index) => {
                    const watched = watchedDefinitions?.[index];
                    return (
                        <AnnotatedItemCard
                            key={field.id}
                            index={index}
                            color="primary"
                            textValue={watched?.definitionText ?? ""}
                            error={
                                errors.definitions?.[index]?.definitionText
                                    ?.message
                            }
                            examples={watched?.examples ?? []}
                            isLoading={isLoading}
                            onTextChange={(value) =>
                                handleDefinitionTextChange(index, value)
                            }
                            onDelete={() => removeDefinition(index)}
                            onAddExample={(text) =>
                                handleAddDefinitionExample(index, text)
                            }
                            onDeleteExample={(exampleIndex) =>
                                handleDeleteDefinitionExample(
                                    index,
                                    exampleIndex
                                )
                            }
                        />
                    );
                })}
            </AnnotatedItemSection>

            <AnnotatedItemSection
                title="Translations"
                color="secondary"
                emptyMessage="No translations yet"
                dialogTitle="Add Translation"
                dialogLabel="Translation"
                dialogMultiline={false}
                itemCount={translationFields.length}
                isLoading={isLoading}
                onAdd={handleAddTranslation}
            >
                {translationFields.map((field, index) => {
                    const watched = watchedTranslations?.[index];
                    return (
                        <AnnotatedItemCard
                            key={field.id}
                            index={index}
                            color="secondary"
                            textValue={watched?.translationText ?? ""}
                            error={
                                errors.translations?.[index]?.translationText
                                    ?.message
                            }
                            examples={watched?.examples ?? []}
                            isLoading={isLoading}
                            onTextChange={(value) =>
                                handleTranslationTextChange(index, value)
                            }
                            onDelete={() => removeTranslation(index)}
                            onAddExample={(text) =>
                                handleAddTranslationExample(index, text)
                            }
                            onDeleteExample={(exampleIndex) =>
                                handleDeleteTranslationExample(
                                    index,
                                    exampleIndex
                                )
                            }
                        />
                    );
                })}
            </AnnotatedItemSection>

            {showFooter && (
                <>
                    {!hasContent && (
                        <Typography
                            variant="body2"
                            color="error"
                            className={styles.errorMessage}
                        >
                            At least one definition or translation is required
                        </Typography>
                    )}

                    <Box className={styles.actions}>
                        <Button onClick={onCancel} disabled={isLoading}>
                            Cancel
                        </Button>
                        <Button
                            type="submit"
                            variant="contained"
                            disabled={isLoading || !isValid}
                        >
                            {isLoading ? "Saving..." : submitLabel}
                        </Button>
                    </Box>
                </>
            )}
        </Box>
    );
};

export type { EntryFormValues, EntryFormOutput };
