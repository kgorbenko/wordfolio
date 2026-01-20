import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Box, TextField, Button } from "@mui/material";

import {
    vocabularySchema,
    VocabularyFormInput,
    VocabularyFormData,
} from "../schemas/vocabularySchemas";
import styles from "./VocabularyForm.module.scss";

export interface VocabularyFormValues {
    readonly name: string;
    readonly description: string | undefined;
}

interface VocabularyFormProps {
    readonly defaultValues?: VocabularyFormValues;
    readonly onSubmit: (data: VocabularyFormData) => void;
    readonly onCancel: () => void;
    readonly submitLabel: string;
    readonly isLoading?: boolean;
}

export const VocabularyForm = ({
    defaultValues,
    onSubmit,
    onCancel,
    submitLabel,
    isLoading = false,
}: VocabularyFormProps) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<VocabularyFormInput, unknown, VocabularyFormData>({
        resolver: zodResolver(vocabularySchema),
        defaultValues: {
            name: defaultValues?.name ?? "",
            description: defaultValues?.description ?? "",
        },
    });

    return (
        <Box
            component="form"
            onSubmit={handleSubmit(onSubmit)}
            noValidate
            className={styles.form}
        >
            <TextField
                autoFocus
                fullWidth
                label="Name"
                disabled={isLoading}
                error={!!errors.name}
                helperText={errors.name?.message}
                {...register("name")}
            />

            <TextField
                fullWidth
                label="Description (optional)"
                multiline
                rows={3}
                disabled={isLoading}
                error={!!errors.description}
                helperText={errors.description?.message}
                {...register("description")}
            />

            <Box className={styles.actions}>
                <Button onClick={onCancel} disabled={isLoading}>
                    Cancel
                </Button>
                <Button type="submit" variant="contained" disabled={isLoading}>
                    {isLoading ? "Saving..." : submitLabel}
                </Button>
            </Box>
        </Box>
    );
};
