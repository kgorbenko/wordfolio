import { Box } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";

import { EmptyState } from "../../../components/common/EmptyState";
import { EntryListItem } from "../../entries/components/EntryListItem";
import { Entry } from "../../../types/entry";
import styles from "./DraftsContent.module.scss";

interface DraftsContentProps {
    readonly entries: Entry[];
    readonly onEntryClick: (id: number) => void;
    readonly onAddDraftClick: () => void;
}

export const DraftsContent = ({
    entries,
    onEntryClick,
    onAddDraftClick,
}: DraftsContentProps) => {
    if (entries.length === 0) {
        return (
            <EmptyState
                icon={
                    <MenuBookIcon
                        sx={{ fontSize: 40, color: "secondary.main" }}
                    />
                }
                title="No Drafts Yet"
                description="Tap the + button to add your first word to drafts."
                actionLabel="Add Draft"
                onAction={onAddDraftClick}
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
