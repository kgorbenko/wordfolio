import FolderIcon from "@mui/icons-material/Folder";
import { CardGrid } from "../../../components/common/CardGrid";
import { EmptyState } from "../../../components/common/EmptyState";
import { CollectionCard } from "./CollectionCard";
import { Collection } from "../types";
import styles from "./CollectionsContent.module.scss";

interface CollectionsContentProps {
    readonly collections: Collection[];
    readonly onCollectionClick: (id: number) => void;
    readonly onCreateClick: () => void;
}

export const CollectionsContent = ({
    collections,
    onCollectionClick,
    onCreateClick,
}: CollectionsContentProps) => {
    if (collections.length === 0) {
        return (
            <EmptyState
                icon={<FolderIcon className={styles.emptyIcon} />}
                title="No Collections Yet"
                description="Create your first collection to organize words."
                actionLabel="Create Collection"
                onAction={onCreateClick}
            />
        );
    }

    return (
        <CardGrid>
            {collections.map((collection) => (
                <CollectionCard
                    key={collection.id}
                    collection={collection}
                    onClick={() => onCollectionClick(collection.id)}
                />
            ))}
        </CardGrid>
    );
};
