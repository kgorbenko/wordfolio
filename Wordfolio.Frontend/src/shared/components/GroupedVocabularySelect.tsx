import { type ReactNode, useMemo } from "react";
import {
    ListSubheader,
    MenuItem,
    Select,
    type SelectChangeEvent,
    type SelectProps,
} from "@mui/material";

import type { CollectionsHierarchy } from "../api/types/collections";
import styles from "./GroupedVocabularySelect.module.scss";

interface DraftsItemConfig {
    readonly label: string;
    readonly value: number;
}

interface GroupedVocabularySelectProps {
    readonly hierarchy: CollectionsHierarchy | undefined;
    readonly value: number | string;
    readonly label: string;
    readonly onChange: (event: SelectChangeEvent<number | string>) => void;
    readonly draftsItem?: DraftsItemConfig;
    readonly excludeVocabularyId?: number;
    readonly size?: SelectProps["size"];
    readonly labelId?: string;
}

export const GroupedVocabularySelect = ({
    hierarchy,
    value,
    label,
    onChange,
    draftsItem,
    excludeVocabularyId,
    size,
    labelId,
}: GroupedVocabularySelectProps) => {
    const groupedItems = useMemo(() => {
        if (!hierarchy) {
            return [];
        }

        const items: ReactNode[] = [];

        if (draftsItem) {
            items.push(
                <MenuItem key="drafts" value={draftsItem.value}>
                    {draftsItem.label}
                </MenuItem>
            );
        } else if (hierarchy.defaultVocabulary) {
            const defaultVocab = hierarchy.defaultVocabulary;
            if (defaultVocab.id !== excludeVocabularyId) {
                items.push(
                    <MenuItem key={defaultVocab.id} value={defaultVocab.id}>
                        Drafts
                    </MenuItem>
                );
            }
        }

        let isFirstGroup = true;

        for (const collection of hierarchy.collections) {
            const filteredVocabularies = collection.vocabularies.filter(
                (vocabulary) => vocabulary.id !== excludeVocabularyId
            );

            if (filteredVocabularies.length === 0) {
                continue;
            }

            items.push(
                <ListSubheader
                    key={`header-${collection.id}`}
                    className={
                        isFirstGroup
                            ? styles.groupHeader
                            : styles.groupHeaderSpaced
                    }
                >
                    {collection.name}
                </ListSubheader>
            );

            for (const vocabulary of filteredVocabularies) {
                items.push(
                    <MenuItem key={vocabulary.id} value={vocabulary.id}>
                        {vocabulary.name}
                    </MenuItem>
                );
            }

            isFirstGroup = false;
        }

        return items;
    }, [draftsItem, excludeVocabularyId, hierarchy]);

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
