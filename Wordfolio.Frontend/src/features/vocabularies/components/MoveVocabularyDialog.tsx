import { useCallback, useEffect, useMemo, useState } from "react";
import {
    Alert,
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    MenuItem,
    TextField,
    Typography,
} from "@mui/material";

import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { useCollectionsHierarchyQuery } from "../../../shared/queries/useCollectionsHierarchyQuery";

export interface MoveVocabularySelectionResult {
    readonly collectionId: number;
}

interface MoveVocabularyDialogProps {
    readonly isOpen: boolean;
    readonly currentCollectionId: number;
    readonly onCancel: () => void;
    readonly onConfirm: (value: MoveVocabularySelectionResult) => void;
}

export const MoveVocabularyDialog = ({
    isOpen,
    currentCollectionId,
    onCancel,
    onConfirm,
}: MoveVocabularyDialogProps) => {
    const [selectedCollectionId, setSelectedCollectionId] = useState<
        number | undefined
    >(undefined);

    const {
        data: hierarchy,
        isLoading: isHierarchyLoading,
        isError: isHierarchyError,
        refetch: refetchHierarchy,
    } = useCollectionsHierarchyQuery();

    useEffect(() => {
        if (isOpen) {
            setSelectedCollectionId(undefined);
        }
    }, [currentCollectionId, isOpen]);

    const moveTargets = useMemo(() => {
        if (!hierarchy) {
            return [];
        }

        return hierarchy.collections.filter(
            (collection) => collection.id !== currentCollectionId
        );
    }, [currentCollectionId, hierarchy]);

    const handleTargetChange = useCallback(
        (event: React.ChangeEvent<HTMLInputElement>) => {
            const value = Number(event.target.value);
            setSelectedCollectionId(Number.isNaN(value) ? undefined : value);
        },
        []
    );

    const handleConfirm = useCallback(() => {
        if (selectedCollectionId === undefined) {
            return;
        }

        onConfirm({ collectionId: selectedCollectionId });
    }, [onConfirm, selectedCollectionId]);

    const confirmDisabled =
        selectedCollectionId === undefined || moveTargets.length === 0;

    const renderContent = () => {
        if (isHierarchyLoading) {
            return <ContentSkeleton variant="form" />;
        }

        if (isHierarchyError || !hierarchy) {
            return (
                <RetryOnError
                    onRetry={() => {
                        void refetchHierarchy();
                    }}
                />
            );
        }

        if (moveTargets.length === 0) {
            return (
                <Alert severity="warning">
                    No other collections available. Create a new collection
                    first.
                </Alert>
            );
        }

        return (
            <>
                <Typography sx={{ mb: 2 }}>
                    Choose a target collection for this vocabulary.
                </Typography>
                <TextField
                    select
                    fullWidth
                    label="Target collection"
                    value={
                        selectedCollectionId !== undefined
                            ? String(selectedCollectionId)
                            : ""
                    }
                    onChange={handleTargetChange}
                >
                    {moveTargets.map((collection) => (
                        <MenuItem
                            key={collection.id}
                            value={String(collection.id)}
                        >
                            {collection.name}
                        </MenuItem>
                    ))}
                </TextField>
            </>
        );
    };

    return (
        <Dialog open={isOpen} onClose={onCancel} fullWidth maxWidth="sm">
            <DialogTitle>Move Vocabulary</DialogTitle>
            <DialogContent>{renderContent()}</DialogContent>
            <DialogActions>
                <Button onClick={onCancel}>Cancel</Button>
                <Button
                    variant="contained"
                    onClick={handleConfirm}
                    disabled={confirmDisabled}
                >
                    Move
                </Button>
            </DialogActions>
        </Dialog>
    );
};
