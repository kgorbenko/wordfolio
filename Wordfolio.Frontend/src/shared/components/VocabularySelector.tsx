import { useCallback, useMemo } from "react";
import {
    FormControl,
    InputLabel,
    ListSubheader,
    MenuItem,
    Select,
    type SelectChangeEvent,
} from "@mui/material";

import type { CollectionsHierarchy } from "../api/types/collections";
import styles from "./VocabularySelector.module.scss";

interface VocabularySelectorProps {
    readonly hierarchy: CollectionsHierarchy | undefined;
    readonly value: number | undefined;
    readonly label: string;
    readonly onChange: (value: number | undefined) => void;
    readonly draftsLabel?: string;
    readonly excludeVocabularyId?: number;
    readonly fullWidth?: boolean;
    readonly className?: string;
}

const DRAFTS_VALUE = 0;

const buildGroupedItems = (
    hierarchy: CollectionsHierarchy,
    excludeVocabularyId: number | undefined,
    draftsLabel: string
) => {
    const showDrafts =
        excludeVocabularyId === undefined ||
        excludeVocabularyId !== hierarchy.defaultVocabulary?.id;

    const draftsItem = showDrafts
        ? [
              <MenuItem key="drafts" value={DRAFTS_VALUE}>
                  {draftsLabel}
              </MenuItem>,
          ]
        : [];

    const collectionGroups = hierarchy.collections.flatMap((collection) => {
        const vocabularies = collection.vocabularies.filter(
            (vocabulary) => vocabulary.id !== excludeVocabularyId
        );

        if (vocabularies.length === 0) {
            return [];
        }

        return [
            <ListSubheader
                key={`header-${collection.id}`}
                className={styles.groupHeader}
                disableSticky
            >
                {collection.name}
            </ListSubheader>,
            ...vocabularies.map((vocabulary) => (
                <MenuItem key={vocabulary.id} value={vocabulary.id}>
                    {vocabulary.name}
                </MenuItem>
            )),
        ];
    });

    return [...draftsItem, ...collectionGroups];
};

export const VocabularySelector = ({
    hierarchy,
    value,
    label,
    onChange,
    draftsLabel = "Drafts",
    excludeVocabularyId,
    fullWidth,
    className,
}: VocabularySelectorProps) => {
    const groupedItems = useMemo(
        () =>
            hierarchy
                ? buildGroupedItems(hierarchy, excludeVocabularyId, draftsLabel)
                : [],
        [draftsLabel, excludeVocabularyId, hierarchy]
    );

    const handleChange = useCallback(
        (event: SelectChangeEvent<number | string>) => {
            const rawValue = Number(event.target.value);
            onChange(rawValue === DRAFTS_VALUE ? undefined : rawValue);
        },
        [onChange]
    );

    return (
        <FormControl fullWidth={fullWidth} className={className}>
            <InputLabel>{label}</InputLabel>
            <Select
                value={value ?? DRAFTS_VALUE}
                label={label}
                onChange={handleChange}
            >
                {groupedItems}
            </Select>
        </FormControl>
    );
};
