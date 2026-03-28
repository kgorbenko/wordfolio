import {
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    ListSubheader,
} from "@mui/material";

import type { CollectionsHierarchy } from "../../api/types/collections";
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
}: VocabularySelectorProps) => (
    <FormControl fullWidth size="small" className={styles.formControl}>
        <InputLabel>Vocabulary</InputLabel>
        <Select<number>
            value={value ?? DRAFTS_SENTINEL}
            label="Vocabulary"
            onChange={(e) => {
                const numValue = Number(e.target.value);
                onChange(numValue === DRAFTS_SENTINEL ? undefined : numValue);
            }}
        >
            <MenuItem value={DRAFTS_SENTINEL}>Drafts — organize later</MenuItem>
            {hierarchy?.collections.map((collection) => [
                <ListSubheader key={`header-${collection.id}`}>
                    {collection.name}
                </ListSubheader>,
                ...collection.vocabularies.map((vocab) => (
                    <MenuItem key={vocab.id} value={vocab.id}>
                        {vocab.name}
                    </MenuItem>
                )),
            ])}
        </Select>
    </FormControl>
);
