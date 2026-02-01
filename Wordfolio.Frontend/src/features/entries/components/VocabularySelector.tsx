import {
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    ListSubheader,
} from "@mui/material";

import { CollectionsHierarchyResponse } from "../../../api/vocabulariesApi";
import styles from "./VocabularySelector.module.scss";

interface VocabularySelectorProps {
    readonly value: number;
    readonly hierarchy: CollectionsHierarchyResponse | undefined;
    readonly onChange: (vocabularyId: number) => void;
}

export const VocabularySelector = ({
    value,
    hierarchy,
    onChange,
}: VocabularySelectorProps) => (
    <FormControl fullWidth size="small" className={styles.formControl}>
        <InputLabel>Vocabulary</InputLabel>
        <Select<number>
            value={value}
            label="Vocabulary"
            onChange={(e) => onChange(Number(e.target.value))}
        >
            <MenuItem value={0}>Drafts — organize later</MenuItem>
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
