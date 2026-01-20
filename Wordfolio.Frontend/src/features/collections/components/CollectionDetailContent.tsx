import MenuBookIcon from "@mui/icons-material/MenuBook";
import { CardGrid } from "../../../components/common/CardGrid";
import { EmptyState } from "../../../components/common/EmptyState";
import { VocabularyCard } from "../../../components/vocabularies/VocabularyCard";
import { Vocabulary } from "../types";
import styles from "./CollectionDetailContent.module.scss";

interface CollectionDetailContentProps {
    readonly vocabularies: Vocabulary[];
    readonly onVocabularyClick: (id: number) => void;
    readonly onCreateVocabularyClick: () => void;
}

export const CollectionDetailContent = ({
    vocabularies,
    onVocabularyClick,
    onCreateVocabularyClick,
}: CollectionDetailContentProps) => {
    return (
        <>
            {vocabularies.length === 0 ? (
                <EmptyState
                    icon={<MenuBookIcon className={styles.emptyIcon} />}
                    title="No Vocabularies Yet"
                    description="Add your first vocabulary - a book, movie, or any source of new words."
                    actionLabel="Add Vocabulary"
                    onAction={onCreateVocabularyClick}
                />
            ) : (
                <CardGrid>
                    {vocabularies.map((vocab) => (
                        <VocabularyCard
                            key={vocab.id}
                            id={vocab.id}
                            name={vocab.name}
                            description={vocab.description ?? undefined}
                            entryCount={vocab.entryCount}
                            onClick={() => onVocabularyClick(vocab.id)}
                        />
                    ))}
                </CardGrid>
            )}
        </>
    );
};
