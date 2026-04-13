import { useCallback, useMemo } from "react";
import {
    ListSubheader,
    MenuItem,
    Select,
    type SelectChangeEvent,
} from "@mui/material";

import type { CollectionsHierarchy } from "../api/types/collections";
import styles from "./VocabularySelector.module.scss";

export const draftsValue = "drafts";

interface VocabularySelectorProps {
    readonly hierarchy: CollectionsHierarchy | undefined;
    readonly value: string | undefined;
    readonly label: string;
    readonly onChange: (value: string) => void;
    readonly excludeVocabularyId?: string;
    readonly fullWidth?: boolean;
    readonly className?: string;
}

const DRAFTS_LABEL = "Drafts — organize later";

const buildGroupedItems = (
    hierarchy: CollectionsHierarchy,
    excludeVocabularyId: string | undefined
) => {
    const showDrafts =
        excludeVocabularyId === undefined ||
        excludeVocabularyId !== hierarchy.defaultVocabulary?.id;

    const draftsItem = showDrafts
        ? [
              <MenuItem key="drafts" value={draftsValue}>
                  {DRAFTS_LABEL}
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
    excludeVocabularyId,
    fullWidth,
    className,
}: VocabularySelectorProps) => {
    const groupedItems = useMemo(
        () =>
            hierarchy ? buildGroupedItems(hierarchy, excludeVocabularyId) : [],
        [excludeVocabularyId, hierarchy]
    );

    const handleChange = useCallback(
        (event: SelectChangeEvent<string>) => {
            onChange(event.target.value);
        },
        [onChange]
    );

    return (
        <Select
            value={value ?? ""}
            label={label}
            onChange={handleChange}
            fullWidth={fullWidth}
            className={className}
            MenuProps={{
                PaperProps: { className: styles.vocabularySelector },
            }}
        >
            {groupedItems}
        </Select>
    );
};
