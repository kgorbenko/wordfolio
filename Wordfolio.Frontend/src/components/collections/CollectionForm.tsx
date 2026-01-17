import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Box, TextField, Button } from "@mui/material";

import {
    collectionSchema,
    CollectionFormInput,
    CollectionFormData,
} from "../../schemas/collectionSchemas";

export interface CollectionFormValues {
    readonly name: string;
    readonly description: string | undefined;
}

interface CollectionFormProps {
    readonly defaultValues?: CollectionFormValues;
    readonly onSubmit: (data: CollectionFormData) => void;
    readonly onCancel: () => void;
    readonly submitLabel: string;
    readonly isLoading?: boolean;
}

export const CollectionForm = ({
    defaultValues,
    onSubmit,
    onCancel,
    submitLabel,
    isLoading = false,
}: CollectionFormProps) => {
    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<CollectionFormInput, unknown, CollectionFormData>({
        resolver: zodResolver(collectionSchema),
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
