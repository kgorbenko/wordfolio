import { useState } from "react";
import { useNavigate, useParams } from "@tanstack/react-router";
import {
    Box,
    Drawer,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Collapse,
    Typography,
    Divider,
    alpha,
    useTheme,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import HomeIcon from "@mui/icons-material/Home";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";

import "./Sidebar.scss";

export const SIDEBAR_WIDTH = 280;

interface Vocabulary {
    readonly id: number;
    readonly name: string;
    readonly entryCount: number;
}

interface Collection {
    readonly id: number;
    readonly name: string;
    readonly vocabularies: Vocabulary[];
}

const stubCollections: Collection[] = [
    {
        id: 1,
        name: "Books",
        vocabularies: [
            { id: 1, name: "Catcher in the Rye", entryCount: 42 },
            { id: 2, name: "1984", entryCount: 28 },
            { id: 3, name: "To Kill a Mockingbird", entryCount: 15 },
        ],
    },
    {
        id: 2,
        name: "Movies",
        vocabularies: [
            { id: 4, name: "The Shawshank Redemption", entryCount: 33 },
            { id: 5, name: "Pulp Fiction", entryCount: 21 },
        ],
    },
    {
        id: 3,
        name: "Work",
        vocabularies: [
            { id: 6, name: "Technical Terms", entryCount: 67 },
            { id: 7, name: "Business English", entryCount: 45 },
        ],
    },
    {
        id: 4,
        name: "Unsorted",
        vocabularies: [{ id: 8, name: "My Words", entryCount: 12 }],
    },
];

interface SidebarProps {
    readonly variant: "permanent" | "temporary";
    readonly open?: boolean;
    readonly onClose?: () => void;
}

export const Sidebar = ({ variant, open, onClose }: SidebarProps) => {
    const theme = useTheme();
    const navigate = useNavigate();
    const params = useParams({ strict: false });

    const currentCollectionId = params.collectionId
        ? Number(params.collectionId)
        : undefined;
    const currentVocabularyId = params.vocabularyId
        ? Number(params.vocabularyId)
        : undefined;

    const [expandedCollections, setExpandedCollections] = useState<number[]>(
        () => {
            if (currentCollectionId) {
                return [currentCollectionId];
            }
            return [1, 4];
        }
    );

    const toggleCollection = (collectionId: number) => {
        setExpandedCollections((prev) =>
            prev.includes(collectionId)
                ? prev.filter((id) => id !== collectionId)
                : [...prev, collectionId]
        );
    };

    const isExpanded = (collectionId: number) =>
        expandedCollections.includes(collectionId);

    const handleNavigation = (callback: () => void) => {
        callback();
        if (variant === "temporary") {
            onClose?.();
        }
    };

    const handleCollectionClick = (
        event: React.MouseEvent,
        collectionId: number
    ) => {
        event.stopPropagation();
        handleNavigation(() => {
            void navigate({
                to: "/collections/$collectionId",
                params: { collectionId: String(collectionId) },
            });
        });
    };

    const handleVocabularyClick = (
        collectionId: number,
        vocabularyId: number
    ) => {
        handleNavigation(() => {
            void navigate({
                to: "/collections/$collectionId/$vocabularyId",
                params: {
                    collectionId: String(collectionId),
                    vocabularyId: String(vocabularyId),
                },
            });
        });
    };

    const handleHomeClick = () => {
        handleNavigation(() => {
            void navigate({ to: "/collections" });
        });
    };

    const drawerContent = (
        <Box className="sidebar">
            <Box
                className="logo-container"
                sx={{ borderBottom: `1px solid ${theme.palette.divider}` }}
                onClick={handleHomeClick}
            >
                <Box
                    className="logo-icon-wrapper"
                    sx={{ bgcolor: "primary.main" }}
                >
                    <MenuBookIcon sx={{ color: "white", fontSize: 18 }} />
                </Box>
                <Typography
                    variant="h6"
                    fontWeight={700}
                    sx={{
                        background: `linear-gradient(135deg, ${theme.palette.primary.main}, ${theme.palette.secondary.main})`,
                        backgroundClip: "text",
                        WebkitBackgroundClip: "text",
                        WebkitTextFillColor: "transparent",
                    }}
                >
                    Wordfolio
                </Typography>
            </Box>

            <Box sx={{ py: 1 }}>
                <ListItemButton
                    onClick={handleHomeClick}
                    sx={{
                        px: 2,
                        py: 1,
                        "&:hover": {
                            bgcolor: alpha(theme.palette.primary.main, 0.04),
                        },
                    }}
                >
                    <ListItemIcon className="nav-icon">
                        <HomeIcon
                            sx={{ color: "text.secondary", fontSize: 20 }}
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

            <Divider />

            <Box className="content">
                <List disablePadding>
                    {stubCollections.map((collection) => (
                        <Box key={collection.id}>
                            <ListItemButton
                                onClick={() => toggleCollection(collection.id)}
                                sx={{
                                    px: 2,
                                    py: 1,
                                    "&:hover": {
                                        bgcolor: alpha(
                                            theme.palette.primary.main,
                                            0.04
                                        ),
                                    },
                                }}
                            >
                                <ListItemIcon className="expand-icon">
                                    {isExpanded(collection.id) ? (
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
                                <ListItemIcon className="folder-icon">
                                    <FolderIcon
                                        sx={{
                                            color:
                                                isExpanded(collection.id) ||
                                                currentCollectionId ===
                                                    collection.id
                                                    ? "primary.main"
                                                    : "text.secondary",
                                            fontSize: 20,
                                        }}
                                    />
                                </ListItemIcon>
                                <ListItemText
                                    primary={collection.name}
                                    onClick={(e) =>
                                        handleCollectionClick(e, collection.id)
                                    }
                                    primaryTypographyProps={{
                                        fontWeight:
                                            isExpanded(collection.id) ||
                                            currentCollectionId ===
                                                collection.id
                                                ? 600
                                                : 500,
                                        fontSize: "0.9rem",
                                        color:
                                            currentCollectionId ===
                                            collection.id
                                                ? "primary.main"
                                                : isExpanded(collection.id)
                                                    ? "text.primary"
                                                    : "text.secondary",
                                        sx: {
                                            "&:hover": {
                                                textDecoration: "underline",
                                            },
                                        },
                                    }}
                                />
                                <Typography
                                    className="nav-count"
                                    variant="caption"
                                    sx={{ color: "text.disabled" }}
                                >
                                    {collection.vocabularies.length}
                                </Typography>
                            </ListItemButton>

                            <Collapse
                                in={isExpanded(collection.id)}
                                timeout="auto"
                                unmountOnExit
                            >
                                <List disablePadding>
                                    {collection.vocabularies.map((vocab) => (
                                        <ListItemButton
                                            key={vocab.id}
                                            selected={
                                                currentVocabularyId === vocab.id
                                            }
                                            onClick={() =>
                                                handleVocabularyClick(
                                                    collection.id,
                                                    vocab.id
                                                )
                                            }
                                            sx={{
                                                pl: 8.5,
                                                py: 0.75,
                                                "&:hover": {
                                                    bgcolor: alpha(
                                                        theme.palette.primary
                                                            .main,
                                                        0.04
                                                    ),
                                                },
                                                "&.Mui-selected": {
                                                    bgcolor: alpha(
                                                        theme.palette.primary
                                                            .main,
                                                        0.08
                                                    ),
                                                    borderRight: `3px solid ${theme.palette.primary.main}`,
                                                    "&:hover": {
                                                        bgcolor: alpha(
                                                            theme.palette
                                                                .primary.main,
                                                            0.12
                                                        ),
                                                    },
                                                },
                                            }}
                                        >
                                            <ListItemIcon className="vocab-icon">
                                                <MenuBookIcon
                                                    sx={{
                                                        color:
                                                            currentVocabularyId ===
                                                            vocab.id
                                                                ? "primary.main"
                                                                : "text.disabled",
                                                        fontSize: 16,
                                                    }}
                                                />
                                            </ListItemIcon>
                                            <ListItemText
                                                primary={vocab.name}
                                                primaryTypographyProps={{
                                                    fontSize: "0.85rem",
                                                    fontWeight:
                                                        currentVocabularyId ===
                                                        vocab.id
                                                            ? 600
                                                            : 400,
                                                    color:
                                                        currentVocabularyId ===
                                                        vocab.id
                                                            ? "primary.main"
                                                            : "text.secondary",
                                                    noWrap: true,
                                                }}
                                            />
                                            <Typography
                                                className="vocab-count"
                                                variant="caption"
                                                sx={{ color: "text.disabled" }}
                                            >
                                                {vocab.entryCount}
                                            </Typography>
                                        </ListItemButton>
                                    ))}
                                </List>
                            </Collapse>
                        </Box>
                    ))}
                </List>
            </Box>
        </Box>
    );

    if (variant === "temporary") {
        return (
            <Drawer
                variant="temporary"
                open={open}
                onClose={onClose}
                ModalProps={{ keepMounted: true }}
                sx={{
                    "& .MuiDrawer-paper": {
                        width: SIDEBAR_WIDTH,
                        boxSizing: "border-box",
                        bgcolor: "background.paper",
                        borderRadius: 0,
                    },
                }}
            >
                {drawerContent}
            </Drawer>
        );
    }

    return (
        <Drawer
            variant="permanent"
            sx={{
                width: SIDEBAR_WIDTH,
                flexShrink: 0,
                "& .MuiDrawer-paper": {
                    width: SIDEBAR_WIDTH,
                    boxSizing: "border-box",
                    borderRight: `1px solid ${theme.palette.divider}`,
                    bgcolor: "background.paper",
                    position: "relative",
                    borderRadius: 0,
                },
            }}
        >
            {drawerContent}
        </Drawer>
    );
};
