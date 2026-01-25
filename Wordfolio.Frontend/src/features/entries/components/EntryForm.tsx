import { forwardRef, useImperativeHandle } from "react";
import { useForm, useFieldArray } from "react-hook-form";
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
} from "../types";
import {
    DefinitionRequest,
    TranslationRequest,
    ExampleRequest,
} from "../../../api/entriesApi";
import { AnnotatedItemSection } from "./AnnotatedItemSection";
import { AnnotatedFieldArray } from "./AnnotatedFieldArray";
import styles from "./EntryForm.module.scss";

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
        const {
            register,
            control,
            handleSubmit,
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
        } = useFieldArray({
            control,
            name: "definitions",
        });

        const {
            fields: translationFields,
            append: appendTranslation,
            remove: removeTranslation,
        } = useFieldArray({
            control,
            name: "translations",
        });

        useImperativeHandle(ref, () => ({
            submit: () => handleSubmit(handleFormSubmit)(),
        }));

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
            appendDefinition(newDefinition, { shouldFocus: true });
        };

        const handleAddTranslation = () => {
            const newTranslation: TranslationItem = {
                id: `trans-${Date.now()}`,
                translationText: "",
                source: "Manual",
                examples: [],
            };
            appendTranslation(newTranslation, { shouldFocus: true });
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
                        {definitionFields.map((field, index) => (
                            <AnnotatedFieldArray
                                key={field.id}
                                control={control}
                                register={register}
                                errors={errors}
                                index={index}
                                color="primary"
                                textFieldPath={`definitions.${index}.definitionText`}
                                examplesPath={`definitions.${index}.examples`}
                                getExampleTextPath={(exIndex) =>
                                    `definitions.${index}.examples.${exIndex}.exampleText`
                                }
                                getTextError={() =>
                                    errors.definitions?.[index]?.definitionText
                                        ?.message
                                }
                                getExampleError={(exIndex) =>
                                    errors.definitions?.[index]?.examples?.[
                                        exIndex
                                    ]?.exampleText?.message
                                }
                                onRemove={() => removeDefinition(index)}
                                isLoading={isLoading}
                            />
                        ))}
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
                        {translationFields.map((field, index) => (
                            <AnnotatedFieldArray
                                key={field.id}
                                control={control}
                                register={register}
                                errors={errors}
                                index={index}
                                color="secondary"
                                textFieldPath={`translations.${index}.translationText`}
                                examplesPath={`translations.${index}.examples`}
                                getExampleTextPath={(exIndex) =>
                                    `translations.${index}.examples.${exIndex}.exampleText`
                                }
                                getTextError={() =>
                                    errors.translations?.[index]
                                        ?.translationText?.message
                                }
                                getExampleError={(exIndex) =>
                                    errors.translations?.[index]?.examples?.[
                                        exIndex
                                    ]?.exampleText?.message
                                }
                                onRemove={() => removeTranslation(index)}
                                isLoading={isLoading}
                            />
                        ))}
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
