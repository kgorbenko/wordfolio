import MenuBookIcon from "@mui/icons-material/MenuBook";

import { EmptyState } from "../../../components/common/EmptyState";

interface DraftsEmptyStateProps {
    readonly onAddDraftClick: () => void;
}

export const DraftsEmptyState = ({
    onAddDraftClick,
}: DraftsEmptyStateProps) => {
    return (
        <EmptyState
            icon={
                <MenuBookIcon sx={{ fontSize: 40, color: "secondary.main" }} />
            }
            title="No Drafts Yet"
            description="Your drafts will appear here when you add your first word. Tap the + button to get started."
            actionLabel="Add Draft"
            onAction={onAddDraftClick}
        />
    );
};
