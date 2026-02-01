import { Box, Divider } from "@mui/material";
import {
    CollectionSummaryResponse,
    VocabularySummaryResponse,
} from "../../../api/vocabulariesApi";
import { SidebarHeader } from "./SidebarHeader";
import { QuickAccessSection } from "./QuickAccessSection";
import { CollectionsList } from "./CollectionsList";
import { CollectionsListSkeleton } from "./skeletons/CollectionsListSkeleton";
import { RetryOnError } from "../../common/RetryOnError";
import styles from "./SidebarContent.module.scss";

interface SidebarContentProps {
    readonly collections: CollectionSummaryResponse[];
    readonly draftsVocabulary: VocabularySummaryResponse | null;
    readonly activeCollectionId: number | undefined;
    readonly activeVocabularyId: number | undefined;
    readonly expandedCollections: readonly number[];
    readonly onToggleCollection: (collectionId: number) => void;
    readonly onCollectionClick: (collectionId: number) => void;
    readonly onVocabularyClick: (
        collectionId: number,
        vocabularyId: number
    ) => void;
    readonly onHomeClick: () => void;
    readonly onAddEntry?: () => void;
    readonly onRetry: () => void;
    readonly isLoading: boolean;
    readonly isError: boolean;
}

export const SidebarContent = ({
    collections,
    draftsVocabulary,
    activeCollectionId,
    activeVocabularyId,
    expandedCollections,
    onToggleCollection,
    onCollectionClick,
    onVocabularyClick,
    onHomeClick,
    onAddEntry,
    onRetry,
    isLoading,
    isError,
}: SidebarContentProps) => {
    const isDefaultVocabularySelected = false;

    return (
        <Box sx={{ display: "flex", flexDirection: "column", height: "100%" }}>
            <SidebarHeader onHomeClick={onHomeClick} onAddEntry={onAddEntry} />

            <QuickAccessSection
                draftsVocabulary={draftsVocabulary}
                isLoading={isLoading}
                isDefaultVocabularySelected={isDefaultVocabularySelected}
                onDraftsClick={() => {}}
                onAllCollectionsClick={onHomeClick}
            />

            <Divider />

            <Box className={styles.collectionsSection}>
                {isLoading ? (
                    <CollectionsListSkeleton />
                ) : isError ? (
                    <RetryOnError
                        title="Failed to Load"
                        description="Unable to load collections."
                        onRetry={onRetry}
                    />
                ) : (
                    <CollectionsList
                        collections={collections}
                        activeCollectionId={activeCollectionId}
                        activeVocabularyId={activeVocabularyId}
                        expandedCollections={expandedCollections}
                        onToggleCollection={onToggleCollection}
                        onCollectionClick={onCollectionClick}
                        onVocabularyClick={onVocabularyClick}
                    />
                )}
            </Box>
        </Box>
    );
};
