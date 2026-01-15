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
    Skeleton,
    alpha,
    useTheme,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import HomeIcon from "@mui/icons-material/Home";
import StarIcon from "@mui/icons-material/Star";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";

import {
    CollectionSummaryResponse,
    VocabularySummaryResponse,
} from "../../api/vocabulariesApi";
import { useCollectionsHierarchyQuery } from "../../queries/useCollectionsHierarchyQuery";

import "./Sidebar.scss";

export const SIDEBAR_WIDTH = 280;

interface SidebarProps {
    readonly variant: "permanent" | "temporary";
    readonly open?: boolean;
    readonly onClose?: () => void;
}

const AllCollectionsButtonSkeleton = () => (
    <ListItemButton sx={{ px: 2, py: 1 }}>
        <ListItemIcon className="nav-icon">
            <Skeleton variant="circular" width={20} height={20} />
        </ListItemIcon>
        <ListItemText primary={<Skeleton variant="text" width="80%" />} />
    </ListItemButton>
);

const DefaultVocabularyPinnedSkeleton = () => (
    <ListItemButton sx={{ px: 2, py: 1 }}>
        <ListItemIcon className="nav-icon">
            <Skeleton variant="circular" width={20} height={20} />
        </ListItemIcon>
        <ListItemText primary={<Skeleton variant="text" width="60%" />} />
        <Skeleton variant="text" width={16} />
    </ListItemButton>
);

interface DefaultVocabularyPinnedProps {
    readonly defaultVocabulary: VocabularySummaryResponse;
    readonly isSelected: boolean;
    readonly onClick: () => void;
}

const DefaultVocabularyPinned = ({
    defaultVocabulary,
    isSelected,
    onClick,
}: DefaultVocabularyPinnedProps) => {
    const theme = useTheme();

    return (
        <ListItemButton
            selected={isSelected}
            onClick={onClick}
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
                        bgcolor: alpha(theme.palette.primary.main, 0.12),
                    },
                },
            }}
        >
            <ListItemIcon className="nav-icon">
                <StarIcon
                    sx={{
                        color: isSelected ? "primary.main" : "warning.main",
                        fontSize: 20,
                    }}
                />
            </ListItemIcon>
            <ListItemText
                primary={defaultVocabulary.name}
                primaryTypographyProps={{
                    fontWeight: isSelected ? 600 : 500,
                    fontSize: "0.9rem",
                    color: isSelected ? "primary.main" : "text.secondary",
                }}
            />
            <Typography
                className="nav-count"
                variant="caption"
                sx={{ color: "text.disabled" }}
            >
                {defaultVocabulary.entryCount}
            </Typography>
        </ListItemButton>
    );
};

const CollectionsListSkeleton = () => (
    <List disablePadding>
        {[1, 2, 3].map((i) => (
            <ListItemButton key={i} sx={{ px: 2, py: 1 }}>
                <ListItemIcon className="expand-icon">
                    <Skeleton variant="circular" width={20} height={20} />
                </ListItemIcon>
                <ListItemIcon className="folder-icon">
                    <Skeleton variant="circular" width={20} height={20} />
                </ListItemIcon>
                <ListItemText
                    primary={<Skeleton variant="text" width="70%" />}
                />
                <Skeleton variant="text" width={16} className="nav-count" />
            </ListItemButton>
        ))}
    </List>
);

const CollectionsListError = () => {
    const theme = useTheme();

    return (
        <Box
            sx={{
                display: "flex",
                flexDirection: "column",
                alignItems: "center",
                py: 3,
                px: 2,
                color: "text.secondary",
            }}
        >
            <ErrorOutlineIcon
                sx={{ fontSize: 32, mb: 1, color: theme.palette.error.light }}
            />
            <Typography variant="body2" textAlign="center">
                Failed to load collections
            </Typography>
        </Box>
    );
};

interface CollectionsListProps {
    readonly collections: CollectionSummaryResponse[];
    readonly currentCollectionId: number | undefined;
    readonly currentVocabularyId: number | undefined;
    readonly expandedCollections: number[];
    readonly onToggleCollection: (collectionId: number) => void;
    readonly onCollectionClick: (
        event: React.MouseEvent,
        collectionId: number
    ) => void;
    readonly onVocabularyClick: (
        collectionId: number,
        vocabularyId: number
    ) => void;
}

const CollectionsList = ({
    collections,
    currentCollectionId,
    currentVocabularyId,
    expandedCollections,
    onToggleCollection,
    onCollectionClick,
    onVocabularyClick,
}: CollectionsListProps) => {
    const theme = useTheme();

    const isExpanded = (collectionId: number) =>
        expandedCollections.includes(collectionId);

    return (
        <List disablePadding>
            {collections.map((collection) => (
                <Box key={collection.id}>
                    <ListItemButton
                        onClick={() => onToggleCollection(collection.id)}
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
                                        currentCollectionId === collection.id
                                            ? "primary.main"
                                            : "text.secondary",
                                    fontSize: 20,
                                }}
                            />
                        </ListItemIcon>
                        <ListItemText
                            primary={collection.name}
                            onClick={(e) => onCollectionClick(e, collection.id)}
                            primaryTypographyProps={{
                                fontWeight:
                                    isExpanded(collection.id) ||
                                    currentCollectionId === collection.id
                                        ? 600
                                        : 500,
                                fontSize: "0.9rem",
                                color:
                                    currentCollectionId === collection.id
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
                                    selected={currentVocabularyId === vocab.id}
                                    onClick={() =>
                                        onVocabularyClick(
                                            collection.id,
                                            vocab.id
                                        )
                                    }
                                    sx={{
                                        pl: 8.5,
                                        py: 0.75,
                                        "&:hover": {
                                            bgcolor: alpha(
                                                theme.palette.primary.main,
                                                0.04
                                            ),
                                        },
                                        "&.Mui-selected": {
                                            bgcolor: alpha(
                                                theme.palette.primary.main,
                                                0.08
                                            ),
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
                                                currentVocabularyId === vocab.id
                                                    ? 600
                                                    : 400,
                                            color:
                                                currentVocabularyId === vocab.id
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
    );
};

export const Sidebar = ({ variant, open, onClose }: SidebarProps) => {
    const theme = useTheme();
    const navigate = useNavigate();
    const params = useParams({ strict: false });
    const { data, isLoading, isError } = useCollectionsHierarchyQuery();

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
            return [];
        }
    );

    const toggleCollection = (collectionId: number) => {
        setExpandedCollections((prev) =>
            prev.includes(collectionId)
                ? prev.filter((id) => id !== collectionId)
                : [...prev, collectionId]
        );
    };

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

    const isDefaultVocabularySelected = false;

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
                {isLoading ? (
                    <>
                        <DefaultVocabularyPinnedSkeleton />
                        <AllCollectionsButtonSkeleton />
                    </>
                ) : (
                    <>
                        {data?.defaultVocabulary && (
                            <DefaultVocabularyPinned
                                defaultVocabulary={data.defaultVocabulary}
                                isSelected={!!isDefaultVocabularySelected}
                                onClick={() => {}}
                            />
                        )}
                        <ListItemButton
                            onClick={handleHomeClick}
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
                            <ListItemIcon className="nav-icon">
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
                    </>
                )}
            </Box>

            <Divider />

            <Box className="content">
                {isLoading ? (
                    <CollectionsListSkeleton />
                ) : isError ? (
                    <CollectionsListError />
                ) : (
                    <CollectionsList
                        collections={data?.collections ?? []}
                        currentCollectionId={currentCollectionId}
                        currentVocabularyId={currentVocabularyId}
                        expandedCollections={expandedCollections}
                        onToggleCollection={toggleCollection}
                        onCollectionClick={handleCollectionClick}
                        onVocabularyClick={handleVocabularyClick}
                    />
                )}
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
