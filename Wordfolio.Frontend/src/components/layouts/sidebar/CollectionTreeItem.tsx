import {
    Box,
    Collapse,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Typography,
    alpha,
    useTheme,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import { CollectionSummaryResponse } from "../../../api/vocabulariesApi";
import { VocabularyTreeItem } from "./VocabularyTreeItem";

interface CollectionTreeItemProps {
    readonly collection: CollectionSummaryResponse;
    readonly isExpanded: boolean;
    readonly isActive: boolean;
    readonly activeVocabularyId: number | undefined;
    readonly onToggle: () => void;
    readonly onCollectionClick: () => void;
    readonly onVocabularyClick: (vocabularyId: number) => void;
}

export const CollectionTreeItem = ({
    collection,
    isExpanded,
    isActive,
    activeVocabularyId,
    onToggle,
    onCollectionClick,
    onVocabularyClick,
}: CollectionTreeItemProps) => {
    const theme = useTheme();

    return (
        <Box>
            <ListItemButton
                onClick={onToggle}
                sx={{
                    px: 2,
                    py: 1,
                    bgcolor: isActive
                        ? alpha(theme.palette.primary.main, 0.08)
                        : "transparent",
                    "&:hover": {
                        bgcolor: isActive
                            ? alpha(theme.palette.primary.main, 0.12)
                            : alpha(theme.palette.primary.main, 0.04),
                    },
                }}
            >
                <ListItemIcon sx={{ minWidth: 36 }}>
                    {isExpanded ? (
                        <ExpandMoreIcon
                            sx={{
                                color: "primary.main",
                                fontSize: 20,
                            }}
                        />
                    ) : (
                        <ChevronRightIcon
                            sx={{
                                color: "text.secondary",
                                fontSize: 20,
                            }}
                        />
                    )}
                </ListItemIcon>
                <ListItemIcon sx={{ minWidth: 32 }}>
                    <FolderIcon
                        sx={{
                            color:
                                isExpanded || isActive
                                    ? "primary.main"
                                    : "text.secondary",
                            fontSize: 20,
                        }}
                    />
                </ListItemIcon>
                <ListItemText
                    primary={collection.name}
                    onClick={(event) => {
                        event.stopPropagation();
                        onCollectionClick();
                    }}
                    primaryTypographyProps={{
                        fontWeight: isExpanded || isActive ? 600 : 500,
                        fontSize: "0.9rem",
                        color: isActive
                            ? "primary.main"
                            : isExpanded
                              ? "text.primary"
                              : "text.secondary",
                        sx: {
                            "&:hover": {
                                textDecoration: "underline",
                            },
                        },
                    }}
                />
                <Typography variant="caption" sx={{ color: "text.disabled" }}>
                    {collection.vocabularies.length}
                </Typography>
            </ListItemButton>

            <Collapse in={isExpanded} timeout="auto" unmountOnExit>
                <List disablePadding>
                    {collection.vocabularies.map((vocab) => (
                        <VocabularyTreeItem
                            key={vocab.id}
                            vocabulary={vocab}
                            isActive={activeVocabularyId === vocab.id}
                            onClick={() => onVocabularyClick(vocab.id)}
                        />
                    ))}
                </List>
            </Collapse>
        </Box>
    );
};
