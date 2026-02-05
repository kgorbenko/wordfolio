import {
    Box,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Typography,
    alpha,
    useTheme,
} from "@mui/material";
import StarIcon from "@mui/icons-material/Star";
import HomeIcon from "@mui/icons-material/Home";
import { VocabularySummaryResponse } from "../../../api/vocabulariesApi";
import {
    AllCollectionsButtonSkeleton,
    DefaultVocabularyPinnedSkeleton,
} from "./skeletons/QuickAccessSkeleton";

interface QuickAccessSectionProps {
    readonly draftsVocabulary: VocabularySummaryResponse | null;
    readonly isLoading: boolean;
    readonly isDraftsVocabularyActive: boolean;
    readonly onDraftsClick: () => void;
    readonly onAllCollectionsClick: () => void;
}

export const QuickAccessSection = ({
    draftsVocabulary,
    isLoading,
    isDraftsVocabularyActive,
    onDraftsClick,
    onAllCollectionsClick,
}: QuickAccessSectionProps) => {
    const theme = useTheme();

    if (isLoading) {
        return (
            <Box sx={{ py: 1 }}>
                <DefaultVocabularyPinnedSkeleton />
                <AllCollectionsButtonSkeleton />
            </Box>
        );
    }

    return (
        <Box sx={{ py: 1 }}>
            {draftsVocabulary && (
                <ListItemButton
                    selected={isDraftsVocabularyActive}
                    onClick={onDraftsClick}
                    sx={{
                        px: 2,
                        py: 1,
                        "&:hover": {
                            bgcolor: alpha(theme.palette.primary.main, 0.04),
                        },
                        "&.Mui-selected": {
                            bgcolor: alpha(theme.palette.primary.main, 0.08),
                            borderRight: `3px solid ${theme.palette.primary.main}`,
                            "&:hover": {
                                bgcolor: alpha(
                                    theme.palette.primary.main,
                                    0.12
                                ),
                            },
                        },
                    }}
                >
                    <ListItemIcon sx={{ minWidth: 36 }}>
                        <StarIcon
                            sx={{
                                color: isDraftsVocabularyActive
                                    ? "primary.main"
                                    : "warning.main",
                                fontSize: 20,
                            }}
                        />
                    </ListItemIcon>
                    <ListItemText
                        primary="Drafts"
                        primaryTypographyProps={{
                            fontWeight: isDraftsVocabularyActive ? 600 : 500,
                            fontSize: "0.9rem",
                            color: isDraftsVocabularyActive
                                ? "primary.main"
                                : "text.secondary",
                        }}
                    />
                    <Typography
                        variant="caption"
                        sx={{ color: "text.disabled" }}
                    >
                        {draftsVocabulary.entryCount}
                    </Typography>
                </ListItemButton>
            )}
            <ListItemButton
                onClick={onAllCollectionsClick}
                sx={{
                    px: 2,
                    py: 1,
                    "&:hover": {
                        bgcolor: alpha(theme.palette.primary.main, 0.04),
                    },
                }}
            >
                <ListItemIcon sx={{ minWidth: 36 }}>
                    <HomeIcon
                        sx={{
                            color: "text.secondary",
                            fontSize: 20,
                        }}
                    />
                </ListItemIcon>
                <ListItemText
                    primary="All Collections"
                    primaryTypographyProps={{
                        fontWeight: 500,
                        fontSize: "0.9rem",
                        color: "text.secondary",
                    }}
                />
            </ListItemButton>
        </Box>
    );
};
