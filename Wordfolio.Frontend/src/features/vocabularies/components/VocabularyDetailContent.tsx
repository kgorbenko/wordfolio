import { Box } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";

import { EmptyState } from "../../../components/common/EmptyState";
import { EntryListItem } from "../../../components/entries/EntryListItem";
import { EntryResponse } from "../../../api/entriesApi"; // Use EntryResponse directly
import styles from "./VocabularyDetailContent.module.scss";

interface VocabularyDetailContentProps {
    readonly entries: EntryResponse[];
    readonly onEntryClick: (id: number) => void;
    readonly onAddWordClick: () => void;
}

export const VocabularyDetailContent = ({
    entries,
    onEntryClick,
    onAddWordClick,
}: VocabularyDetailContentProps) => {
    if (entries.length === 0) {
        return (
            <EmptyState
                icon={
                    <MenuBookIcon
                        sx={{ fontSize: 40, color: "secondary.main" }}
                    />
                }
                title="No Words Yet"
                description="Tap the + button to add your first word to this vocabulary."
                actionLabel="Add Word"
                onAction={onAddWordClick}
            />
        );
    }

    return (
        <Box className={styles.entriesList}>
            {entries.map((entry) => (
                <EntryListItem
                    key={entry.id}
                    id={entry.id}
                    entryText={entry.entryText}
                    firstDefinition={entry.definitions[0]?.definitionText}
                    firstTranslation={entry.translations[0]?.translationText}
                    createdAt={entry.createdAt}
                    onClick={() => onEntryClick(entry.id)}
                />
            ))}
        </Box>
    );
};
