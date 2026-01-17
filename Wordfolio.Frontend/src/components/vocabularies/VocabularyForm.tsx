import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Box, TextField, Button } from "@mui/material";

import {
    vocabularySchema,
    VocabularyFormInput,
    VocabularyFormData,
} from "../../schemas/vocabularySchemas";

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
            sx={{
                display: "flex",
                flexDirection: "column",
                gap: 2,
                maxWidth: 800,
            }}
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

            <Box sx={{ display: "flex", gap: 2, justifyContent: "flex-end" }}>
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
