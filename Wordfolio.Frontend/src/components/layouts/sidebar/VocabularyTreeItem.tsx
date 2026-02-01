import {
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Typography,
    alpha,
    useTheme,
} from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import { VocabularySummaryResponse } from "../../../api/vocabulariesApi";

interface VocabularyTreeItemProps {
    readonly vocabulary: VocabularySummaryResponse;
    readonly isActive: boolean;
    readonly onClick: () => void;
}

export const VocabularyTreeItem = ({
    vocabulary,
    isActive,
    onClick,
}: VocabularyTreeItemProps) => {
    const theme = useTheme();

    return (
        <ListItemButton
            selected={isActive}
            onClick={onClick}
            sx={{
                pl: 8.5,
                py: 0.75,
                "&:hover": {
                    bgcolor: alpha(theme.palette.primary.main, 0.04),
                },
                "&.Mui-selected": {
                    bgcolor: alpha(theme.palette.primary.main, 0.08),
                    borderRight: `3px solid ${theme.palette.primary.main}`,
                    "&:hover": {
                        bgcolor: alpha(theme.palette.primary.main, 0.12),
                    },
                },
            }}
        >
            <ListItemIcon sx={{ minWidth: 28 }}>
                <MenuBookIcon
                    sx={{
                        color: isActive ? "primary.main" : "text.disabled",
                        fontSize: 16,
                    }}
                />
            </ListItemIcon>
            <ListItemText
                primary={vocabulary.name}
                primaryTypographyProps={{
                    fontSize: "0.85rem",
                    fontWeight: isActive ? 600 : 400,
                    color: isActive ? "primary.main" : "text.secondary",
                    noWrap: true,
                }}
            />
            <Typography
                variant="caption"
                sx={{ color: "text.disabled", ml: 1 }}
            >
                {vocabulary.entryCount}
            </Typography>
        </ListItemButton>
    );
};
