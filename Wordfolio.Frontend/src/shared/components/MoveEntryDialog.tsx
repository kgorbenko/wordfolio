import { useCallback, useEffect, useMemo, useState } from "react";
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    Typography,
} from "@mui/material";

import { ContentSkeleton } from "./ContentSkeleton";
import { draftsValue, VocabularySelector } from "./VocabularySelector";
import { RetryOnError } from "./RetryOnError";
import { useCollectionsHierarchyQuery } from "../api/queries/collections";

export interface MoveEntrySelectionResult {
    readonly vocabularyId: number;
    readonly isDefault: boolean;
    readonly collectionId: number | null;
}

interface MoveEntryDialogProps {
    readonly isOpen: boolean;
    readonly currentVocabularyId: number;
    readonly onCancel: () => void;
    readonly onConfirm: (value: MoveEntrySelectionResult) => void;
}

export const MoveEntryDialog = ({
    isOpen,
    currentVocabularyId,
    onCancel,
    onConfirm,
}: MoveEntryDialogProps) => {
    const [selectedVocabularyId, setSelectedVocabularyId] = useState<
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
            setSelectedVocabularyId(undefined);
        }
    }, [currentVocabularyId, isOpen]);

    const hasTargets = useMemo(() => {
        if (!hierarchy) {
            return false;
        }

        const hasDrafts =
            hierarchy.defaultVocabulary?.id !== currentVocabularyId;

        const hasCollectionVocabularies = hierarchy.collections.some(
            (collection) =>
                collection.vocabularies.some(
                    (vocabulary) => vocabulary.id !== currentVocabularyId
                )
        );

        return hasDrafts || hasCollectionVocabularies;
    }, [currentVocabularyId, hierarchy]);

    const resolveSelection = useCallback(
        (vocabularyId: number): MoveEntrySelectionResult | undefined => {
            if (!hierarchy) {
                return undefined;
            }

            if (vocabularyId === draftsValue) {
                return {
                    vocabularyId: hierarchy.defaultVocabulary?.id ?? 0,
                    isDefault: true,
                    collectionId: null,
                };
            }

            for (const collection of hierarchy.collections) {
                const vocabulary = collection.vocabularies.find(
                    (vocabulary) => vocabulary.id === vocabularyId
                );
                if (vocabulary) {
                    return {
                        vocabularyId,
                        isDefault: false,
                        collectionId: collection.id,
                    };
                }
            }

            return undefined;
        },
        [hierarchy]
    );

    const handleTargetChange = useCallback((value: number) => {
        setSelectedVocabularyId(value);
    }, []);

    const handleConfirm = useCallback(() => {
        if (selectedVocabularyId === undefined) {
            return;
        }

        const selection = resolveSelection(selectedVocabularyId);
        if (!selection) {
            return;
        }

        onConfirm(selection);
    }, [onConfirm, resolveSelection, selectedVocabularyId]);

    const confirmDisabled = !hasTargets || selectedVocabularyId === undefined;

    const renderContent = useCallback(() => {
        if (isHierarchyLoading) {
            return <ContentSkeleton variant="form" />;
        }

        if (isHierarchyError || !hierarchy) {
            return (
                <RetryOnError
                    title="Failed to Load Vocabularies"
                    description="Something went wrong while loading target vocabularies. Please try again."
                    onRetry={() => {
                        void refetchHierarchy();
                    }}
                />
            );
        }

        return (
            <>
                <Typography sx={{ mb: 2 }}>
                    Choose a target vocabulary for this entry.
                </Typography>
                {!hasTargets ? (
                    <Typography color="text.secondary">
                        There are no other vocabularies available for this
                        entry.
                    </Typography>
                ) : null}
                <VocabularySelector
                    hierarchy={hierarchy}
                    value={selectedVocabularyId}
                    label="Target vocabulary"
                    onChange={handleTargetChange}
                    excludeVocabularyId={currentVocabularyId}
                    fullWidth
                />
            </>
        );
    }, [
        currentVocabularyId,
        handleTargetChange,
        hasTargets,
        hierarchy,
        isHierarchyError,
        isHierarchyLoading,
        refetchHierarchy,
        selectedVocabularyId,
    ]);

    return (
        <Dialog open={isOpen} onClose={onCancel} fullWidth maxWidth="sm">
            <DialogTitle>Move Entry</DialogTitle>
            <DialogContent>{renderContent()}</DialogContent>
            <DialogActions>
                <Button variant="outlined" onClick={onCancel}>
                    Cancel
                </Button>
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
