import { useCallback } from "react";
import { FormControl, InputLabel, type SelectChangeEvent } from "@mui/material";

import type { CollectionsHierarchy } from "../../api/types/collections";
import { GroupedVocabularySelect } from "../GroupedVocabularySelect";
import styles from "./VocabularySelector.module.scss";

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
    const defaultVocabularyId = hierarchy?.defaultVocabulary?.id;

    const handleChange = useCallback(
        (event: SelectChangeEvent<number | string>) => {
            const numValue = Number(event.target.value);
            onChange(numValue === defaultVocabularyId ? undefined : numValue);
        },
        [defaultVocabularyId, onChange]
    );

    return (
        <FormControl fullWidth size="small" className={styles.formControl}>
            <InputLabel>Vocabulary</InputLabel>
            <GroupedVocabularySelect
                hierarchy={hierarchy}
                value={value ?? defaultVocabularyId ?? ""}
                label="Vocabulary"
                onChange={handleChange}
                draftsLabel="Drafts — organize later"
                size="small"
            />
        </FormControl>
    );
};
