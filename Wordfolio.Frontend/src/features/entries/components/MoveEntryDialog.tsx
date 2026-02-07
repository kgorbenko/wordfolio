import { useCallback, useEffect, useMemo, useState } from "react";
import {
    Button,
    Dialog,
    DialogActions,
    DialogContent,
    DialogTitle,
    FormControl,
    InputLabel,
    MenuItem,
    Select,
    SelectChangeEvent,
    Typography,
} from "@mui/material";

import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { useCollectionsHierarchyQuery } from "../../../queries/useCollectionsHierarchyQuery";

export interface MoveSelectionResult {
    readonly vocabularyId: number;
    readonly isDefault: boolean;
    readonly collectionId: number | null;
}

interface MoveDialogTarget extends MoveSelectionResult {
    readonly label: string;
}

interface MoveEntryDialogProps {
    readonly isOpen: boolean;
    readonly currentVocabularyId: number;
    readonly onCancel: () => void;
    readonly onConfirm: (value: MoveSelectionResult) => void;
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

    const moveTargets = useMemo<MoveDialogTarget[]>(() => {
        if (!hierarchy) {
            return [];
        }

        const defaultTarget = hierarchy.defaultVocabulary
            ? [
                  {
                      vocabularyId: hierarchy.defaultVocabulary.id,
                      isDefault: true,
                      collectionId: null,
                      label: `Drafts - ${hierarchy.defaultVocabulary.name}`,
                  },
              ]
            : [];

        const collectionTargets = hierarchy.collections.flatMap((collection) =>
            collection.vocabularies.map((vocabulary) => ({
                vocabularyId: vocabulary.id,
                isDefault: false,
                collectionId: collection.id,
                label: `${collection.name} - ${vocabulary.name}`,
            }))
        );

        return [...defaultTarget, ...collectionTargets].filter(
            (target) => target.vocabularyId !== currentVocabularyId
        );
    }, [currentVocabularyId, hierarchy]);

    const selectedTarget = useMemo(
        () =>
            moveTargets.find(
                (target) => target.vocabularyId === selectedVocabularyId
            ),
        [moveTargets, selectedVocabularyId]
    );

    const handleTargetChange = useCallback(
        (event: SelectChangeEvent<string>) => {
            const value = Number(event.target.value);
            setSelectedVocabularyId(Number.isNaN(value) ? undefined : value);
        },
        []
    );

    const handleConfirm = useCallback(() => {
        if (!selectedTarget) {
            return;
        }

        onConfirm({
            vocabularyId: selectedTarget.vocabularyId,
            isDefault: selectedTarget.isDefault,
            collectionId: selectedTarget.collectionId,
        });
    }, [onConfirm, selectedTarget]);

    const confirmDisabled = !selectedTarget || moveTargets.length === 0;

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
                {moveTargets.length === 0 ? (
                    <Typography color="text.secondary">
                        There are no other vocabularies available for this
                        entry.
                    </Typography>
                ) : null}
                <FormControl fullWidth>
                    <InputLabel id="move-entry-target-select-label">
                        Target vocabulary
                    </InputLabel>
                    <Select
                        labelId="move-entry-target-select-label"
                        value={
                            selectedVocabularyId !== undefined
                                ? String(selectedVocabularyId)
                                : ""
                        }
                        label="Target vocabulary"
                        onChange={handleTargetChange}
                    >
                        {moveTargets.map((target) => (
                            <MenuItem
                                key={target.vocabularyId}
                                value={String(target.vocabularyId)}
                            >
                                {target.label}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
            </>
        );
    }, [
        handleTargetChange,
        hierarchy,
        isHierarchyError,
        isHierarchyLoading,
        moveTargets,
        refetchHierarchy,
        selectedVocabularyId,
    ]);

    return (
        <Dialog open={isOpen} onClose={onCancel} fullWidth maxWidth="sm">
            <DialogTitle>Move Entry</DialogTitle>
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
