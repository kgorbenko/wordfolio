import { List } from "@mui/material";
import type { CollectionWithVocabularies } from "../../../types/collectionsHierarchy";
import { CollectionTreeItem } from "./CollectionTreeItem";
import styles from "./CollectionsList.module.scss";

interface CollectionsListProps {
    readonly collections: CollectionWithVocabularies[];
    readonly activeCollectionId: number | undefined;
    readonly activeVocabularyId: number | undefined;
    readonly expandedCollections: readonly number[];
    readonly onToggleCollection: (collectionId: number) => void;
    readonly onCollectionClick: (collectionId: number) => void;
    readonly onVocabularyClick: (
        collectionId: number,
        vocabularyId: number
    ) => void;
}

export const CollectionsList = ({
    collections,
    activeCollectionId,
    activeVocabularyId,
    expandedCollections,
    onToggleCollection,
    onCollectionClick,
    onVocabularyClick,
}: CollectionsListProps) => {
    const isExpanded = (collectionId: number) =>
        expandedCollections.includes(collectionId);

    return (
        <List disablePadding className={styles.list}>
            {collections.map((collection) => (
                <CollectionTreeItem
                    key={collection.id}
                    collection={collection}
                    isExpanded={isExpanded(collection.id)}
                    isActive={activeCollectionId === collection.id}
                    activeVocabularyId={activeVocabularyId}
                    onToggle={() => onToggleCollection(collection.id)}
                    onCollectionClick={() => onCollectionClick(collection.id)}
                    onVocabularyClick={(vocabularyId) =>
                        onVocabularyClick(collection.id, vocabularyId)
                    }
                />
            ))}
        </List>
    );
};
