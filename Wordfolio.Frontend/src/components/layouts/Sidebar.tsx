import { useState } from "react";
import { useNavigate, useParams, useMatch } from "@tanstack/react-router";
import { Drawer, useTheme } from "@mui/material";

import { collectionsPath } from "../../routes/_authenticated/collections/routes";
import { collectionDetailPath } from "../../routes/_authenticated/collections/routes";
import { vocabularyDetailPath } from "../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import {
    draftsPath,
    draftsRouteIds,
} from "../../routes/_authenticated/drafts/routes";
import { useCollectionsHierarchyQuery } from "../../queries/useCollectionsHierarchyQuery";
import { SidebarContent } from "./sidebar/SidebarContent";
import styles from "./Sidebar.module.scss";

const sidebarWidth = 280;

interface SidebarProps {
    readonly variant: "permanent" | "temporary";
    readonly open?: boolean;
    readonly onClose?: () => void;
    readonly onAddEntry?: () => void;
}

export const Sidebar = ({
    variant,
    open,
    onClose,
    onAddEntry,
}: SidebarProps) => {
    const theme = useTheme();
    const navigate = useNavigate();
    const params = useParams({ strict: false });
    const { data, isLoading, isError, refetch } =
        useCollectionsHierarchyQuery();

    const activeCollectionId = params.collectionId
        ? Number(params.collectionId)
        : undefined;
    const activeVocabularyId = params.vocabularyId
        ? Number(params.vocabularyId)
        : undefined;

    const [expandedCollections, setExpandedCollections] = useState<number[]>(
        () => {
            if (activeCollectionId) {
                return [activeCollectionId];
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

    const handleCollectionClick = (collectionId: number) => {
        handleNavigation(() => {
            void navigate(collectionDetailPath(collectionId));
        });
    };

    const handleVocabularyClick = (
        collectionId: number,
        vocabularyId: number
    ) => {
        handleNavigation(() => {
            void navigate(vocabularyDetailPath(collectionId, vocabularyId));
        });
    };

    const handleHomeClick = () => {
        handleNavigation(() => {
            void navigate(collectionsPath());
        });
    };

    const handleDraftsClick = () => {
        handleNavigation(() => {
            void navigate(draftsPath());
        });
    };

    const draftsMatch = useMatch({
        from: draftsRouteIds.list,
        shouldThrow: false,
    });
    const isDraftsVocabularyActive = draftsMatch !== undefined;

    const drawerContent = (
        <div className={styles.sidebar}>
            <SidebarContent
                collections={data?.collections ?? []}
                draftsVocabulary={data?.defaultVocabulary ?? null}
                activeCollectionId={activeCollectionId}
                activeVocabularyId={activeVocabularyId}
                expandedCollections={expandedCollections}
                isDraftsVocabularyActive={isDraftsVocabularyActive}
                onToggleCollection={toggleCollection}
                onCollectionClick={handleCollectionClick}
                onVocabularyClick={handleVocabularyClick}
                onHomeClick={handleHomeClick}
                onDraftsClick={handleDraftsClick}
                onAddEntry={onAddEntry}
                onRetry={() => void refetch()}
                isLoading={isLoading}
                isError={isError}
            />
        </div>
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
                        width: sidebarWidth,
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
                width: sidebarWidth,
                flexShrink: 0,
                "& .MuiDrawer-paper": {
                    width: sidebarWidth,
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
