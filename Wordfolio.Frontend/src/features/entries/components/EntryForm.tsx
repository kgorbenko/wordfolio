import { useEffect, useState, forwardRef, useImperativeHandle } from "react";
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

interface ExampleError {
    exampleText?: { message?: string };
}

type ExampleErrorsArray = (ExampleError | undefined)[] | undefined;

export interface EntryFormHandle {
    submit: () => void;
}

export interface EntryFormProps {
    readonly defaultValues?: EntryFormValues;
    readonly onSubmit: (data: EntryFormOutput) => void;
    readonly onCancel: () => void;
    readonly submitLabel: string;
    readonly isLoading?: boolean;
    readonly showEntryText?: boolean;
    readonly showFooter?: boolean;
}

interface AutoFocusState {
    definitionIndex?: number;
    translationIndex?: number;
    exampleInDefinition?: { defIndex: number; exampleIndex: number };
    exampleInTranslation?: { transIndex: number; exampleIndex: number };
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
        entryText: data.entryText,
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

export const EntryForm = forwardRef<EntryFormHandle, EntryFormProps>(
    (
        {
            defaultValues,
            onSubmit,
            onCancel,
            submitLabel,
            isLoading = false,
            showEntryText = true,
            showFooter = true,
        },
        ref
    ) => {
        const [autoFocus, setAutoFocus] = useState<AutoFocusState>({});

        const {
            register,
            control,
            handleSubmit,
            setValue,
            getValues,
            formState: { errors },
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

        useImperativeHandle(ref, () => ({
            submit: () => handleSubmit(handleFormSubmit)(),
        }));

        useEffect(() => {
            if (
                autoFocus.definitionIndex !== undefined ||
                autoFocus.translationIndex !== undefined ||
                autoFocus.exampleInDefinition !== undefined ||
                autoFocus.exampleInTranslation !== undefined
            ) {
                const timer = setTimeout(() => setAutoFocus({}), 100);
                return () => clearTimeout(timer);
            }
        }, [autoFocus]);

        const handleFormSubmit = (data: EntryFormData) => {
            onSubmit(mapToOutput(data));
        };

        const handleAddDefinition = () => {
            const newDefinition: DefinitionItem = {
                id: `def-${Date.now()}`,
                definitionText: "",
                source: "Manual",
                examples: [],
            };
            appendDefinition(newDefinition);
            setAutoFocus({ definitionIndex: definitionFields.length });
        };

        const handleAddTranslation = () => {
            const newTranslation: TranslationItem = {
                id: `trans-${Date.now()}`,
                translationText: "",
                source: "Manual",
                examples: [],
            };
            appendTranslation(newTranslation);
            setAutoFocus({ translationIndex: translationFields.length });
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

        const handleAddDefinitionExample = (defIndex: number) => {
            const definition = getValues(`definitions.${defIndex}`);
            const newExample: ExampleItem = {
                id: `ex-${Date.now()}`,
                exampleText: "",
                source: "Custom",
            };
            const newExampleIndex = definition.examples.length;
            updateDefinition(defIndex, {
                ...definition,
                examples: [...definition.examples, newExample],
            });
            setAutoFocus({
                exampleInDefinition: {
                    defIndex,
                    exampleIndex: newExampleIndex,
                },
            });
        };

        const handleAddTranslationExample = (transIndex: number) => {
            const translation = getValues(`translations.${transIndex}`);
            const newExample: ExampleItem = {
                id: `ex-${Date.now()}`,
                exampleText: "",
                source: "Custom",
            };
            const newExampleIndex = translation.examples.length;
            updateTranslation(transIndex, {
                ...translation,
                examples: [...translation.examples, newExample],
            });
            setAutoFocus({
                exampleInTranslation: {
                    transIndex,
                    exampleIndex: newExampleIndex,
                },
            });
        };

        const handleDefinitionExampleTextChange = (
            defIndex: number,
            exampleIndex: number,
            value: string
        ) => {
            setValue(
                `definitions.${defIndex}.examples.${exampleIndex}.exampleText`,
                value,
                { shouldValidate: true }
            );
        };

        const handleTranslationExampleTextChange = (
            transIndex: number,
            exampleIndex: number,
            value: string
        ) => {
            setValue(
                `translations.${transIndex}.examples.${exampleIndex}.exampleText`,
                value,
                { shouldValidate: true }
            );
        };

        const handleDeleteDefinitionExample = (
            defIndex: number,
            exampleIndex: number
        ) => {
            const definition = getValues(`definitions.${defIndex}`);
            updateDefinition(defIndex, {
                ...definition,
                examples: definition.examples.filter(
                    (_, i) => i !== exampleIndex
                ),
            });
        };

        const handleDeleteTranslationExample = (
            transIndex: number,
            exampleIndex: number
        ) => {
            const translation = getValues(`translations.${transIndex}`);
            updateTranslation(transIndex, {
                ...translation,
                examples: translation.examples.filter(
                    (_, i) => i !== exampleIndex
                ),
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
                        data-testid="entry-text-field"
                    />
                )}

                <div data-testid="definitions-section">
                    <AnnotatedItemSection
                        title="Definitions"
                        color="primary"
                        emptyMessage="No definitions yet"
                        itemCount={definitionFields.length}
                        isLoading={isLoading}
                        onAdd={handleAddDefinition}
                    >
                        {definitionFields.map((field, index) => {
                            const watched = watchedDefinitions?.[index];
                            const shouldAutoFocus =
                                autoFocus.definitionIndex === index;
                            const exampleAutoFocusIndex =
                                autoFocus.exampleInDefinition?.defIndex ===
                                index
                                    ? autoFocus.exampleInDefinition.exampleIndex
                                    : undefined;
                            return (
                                <AnnotatedItemCard
                                    key={field.id}
                                    index={index}
                                    color="primary"
                                    textValue={watched?.definitionText ?? ""}
                                    error={
                                        errors.definitions?.[index]
                                            ?.definitionText?.message
                                    }
                                    examples={watched?.examples ?? []}
                                    exampleErrors={
                                        errors.definitions?.[index]
                                            ?.examples as ExampleErrorsArray
                                    }
                                    autoFocus={shouldAutoFocus}
                                    autoFocusExampleIndex={
                                        exampleAutoFocusIndex
                                    }
                                    isLoading={isLoading}
                                    onTextChange={(value) =>
                                        handleDefinitionTextChange(index, value)
                                    }
                                    onDelete={() => removeDefinition(index)}
                                    onAddExample={() =>
                                        handleAddDefinitionExample(index)
                                    }
                                    onExampleTextChange={(
                                        exampleIndex,
                                        value
                                    ) =>
                                        handleDefinitionExampleTextChange(
                                            index,
                                            exampleIndex,
                                            value
                                        )
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
                </div>

                <div data-testid="translations-section">
                    <AnnotatedItemSection
                        title="Translations"
                        color="secondary"
                        emptyMessage="No translations yet"
                        itemCount={translationFields.length}
                        isLoading={isLoading}
                        onAdd={handleAddTranslation}
                    >
                        {translationFields.map((field, index) => {
                            const watched = watchedTranslations?.[index];
                            const shouldAutoFocus =
                                autoFocus.translationIndex === index;
                            const exampleAutoFocusIndex =
                                autoFocus.exampleInTranslation?.transIndex ===
                                index
                                    ? autoFocus.exampleInTranslation
                                          .exampleIndex
                                    : undefined;
                            return (
                                <AnnotatedItemCard
                                    key={field.id}
                                    index={index}
                                    color="secondary"
                                    textValue={watched?.translationText ?? ""}
                                    error={
                                        errors.translations?.[index]
                                            ?.translationText?.message
                                    }
                                    examples={watched?.examples ?? []}
                                    exampleErrors={
                                        errors.translations?.[index]
                                            ?.examples as ExampleErrorsArray
                                    }
                                    autoFocus={shouldAutoFocus}
                                    autoFocusExampleIndex={
                                        exampleAutoFocusIndex
                                    }
                                    isLoading={isLoading}
                                    onTextChange={(value) =>
                                        handleTranslationTextChange(
                                            index,
                                            value
                                        )
                                    }
                                    onDelete={() => removeTranslation(index)}
                                    onAddExample={() =>
                                        handleAddTranslationExample(index)
                                    }
                                    onExampleTextChange={(
                                        exampleIndex,
                                        value
                                    ) =>
                                        handleTranslationExampleTextChange(
                                            index,
                                            exampleIndex,
                                            value
                                        )
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
                </div>

                {showFooter && (
                    <>
                        {!hasContent && (
                            <Typography
                                variant="body2"
                                color="error"
                                className={styles.errorMessage}
                            >
                                At least one definition or translation is
                                required
                            </Typography>
                        )}

                        <Box className={styles.actions}>
                            <Button onClick={onCancel} disabled={isLoading}>
                                Cancel
                            </Button>
                            <Button
                                type="submit"
                                variant="contained"
                                disabled={isLoading}
                            >
                                {isLoading ? "Saving..." : submitLabel}
                            </Button>
                        </Box>
                    </>
                )}
            </Box>
        );
    }
);

export type { EntryFormValues, EntryFormOutput };
