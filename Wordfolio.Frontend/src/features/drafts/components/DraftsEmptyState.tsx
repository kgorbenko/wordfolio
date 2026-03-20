import MenuBookIcon from "@mui/icons-material/MenuBook";

import { EmptyState } from "../../../shared/components/EmptyState";

export const DraftsEmptyState = () => (
    <EmptyState
        icon={<MenuBookIcon sx={{ fontSize: 40, color: "secondary.main" }} />}
        title="No Drafts Yet"
        description="Your drafts will appear here when you add your first word. Tap the + button to get started."
    />
);
