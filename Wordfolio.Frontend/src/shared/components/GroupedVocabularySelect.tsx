import { useMemo } from "react";
import {
    ListSubheader,
    MenuItem,
    Select,
    type SelectChangeEvent,
    type SelectProps,
} from "@mui/material";

import type { CollectionsHierarchy } from "../api/types/collections";
import styles from "./GroupedVocabularySelect.module.scss";

interface GroupedVocabularySelectProps {
    readonly hierarchy: CollectionsHierarchy | undefined;
    readonly value: number | string;
    readonly label: string;
    readonly onChange: (event: SelectChangeEvent<number | string>) => void;
    readonly draftsLabel?: string;
    readonly draftsValue?: number | string;
    readonly excludeVocabularyId?: number;
    readonly size?: SelectProps["size"];
    readonly labelId?: string;
}

const buildGroupedItems = (
    hierarchy: CollectionsHierarchy,
    excludeVocabularyId: number | undefined,
    draftsLabel: string,
    draftsValue: number | string | undefined
) => {
    const defaultVocabulary = hierarchy.defaultVocabulary;

    const resolvedDraftsValue =
        draftsValue !== undefined
            ? draftsValue !== excludeVocabularyId
                ? draftsValue
                : undefined
            : defaultVocabulary && defaultVocabulary.id !== excludeVocabularyId
              ? defaultVocabulary.id
              : undefined;

    const draftsItem =
        resolvedDraftsValue !== undefined
            ? [
                  <MenuItem key="drafts" value={resolvedDraftsValue}>
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

export const GroupedVocabularySelect = ({
    hierarchy,
    value,
    label,
    onChange,
    draftsLabel = "Drafts",
    draftsValue,
    excludeVocabularyId,
    size,
    labelId,
}: GroupedVocabularySelectProps) => {
    const groupedItems = useMemo(
        () =>
            hierarchy
                ? buildGroupedItems(
                      hierarchy,
                      excludeVocabularyId,
                      draftsLabel,
                      draftsValue
                  )
                : [],
        [draftsLabel, draftsValue, excludeVocabularyId, hierarchy]
    );

    return (
        <Select
            value={value}
            label={label}
            onChange={onChange}
            size={size}
            labelId={labelId}
        >
            {groupedItems}
        </Select>
    );
};
