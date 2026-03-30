import { useCallback } from "react";
import { FormControl, InputLabel, type SelectChangeEvent } from "@mui/material";

import type { CollectionsHierarchy } from "../../api/types/collections";
import { GroupedVocabularySelect } from "../GroupedVocabularySelect";
import styles from "./VocabularySelector.module.scss";

const DRAFTS_SENTINEL = 0;

interface VocabularySelectorProps {
    readonly value: number | undefined;
    readonly hierarchy: CollectionsHierarchy | undefined;
    readonly onChange: (vocabularyId: number | undefined) => void;
}

export const VocabularySelector = ({
    value,
    hierarchy,
    onChange,
}: VocabularySelectorProps) => {
    const handleChange = useCallback(
        (event: SelectChangeEvent<number | string>) => {
            const numValue = Number(event.target.value);
            onChange(numValue === DRAFTS_SENTINEL ? undefined : numValue);
        },
        [onChange]
    );

    return (
        <FormControl fullWidth size="small" className={styles.formControl}>
            <InputLabel>Vocabulary</InputLabel>
            <GroupedVocabularySelect
                hierarchy={hierarchy}
                value={value ?? DRAFTS_SENTINEL}
                label="Vocabulary"
                onChange={handleChange}
                draftsItem={{
                    label: "Drafts — organize later",
                    value: DRAFTS_SENTINEL,
                }}
                size="small"
            />
        </FormControl>
    );
};
