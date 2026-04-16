import { useEffect } from "react";
import { Box, Button, Typography } from "@mui/material";
import SchoolIcon from "@mui/icons-material/School";

import { practiceRouteApi, vocabularyDetailPath } from "../routes";
import { usePracticeStore } from "../stores/practiceStore";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { useVocabularyEntriesQuery } from "../../../shared/api/queries/entries";
import { useVocabularyDetailQuery } from "../../../shared/api/queries/vocabularies";
import { FlashCard } from "../components/FlashCard";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { EmptyState } from "../../../shared/components/EmptyState";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { TopBarBreadcrumbs } from "../../../shared/components/layouts/TopBarBreadcrumbs";

import styles from "./PracticePage.module.scss";

export const PracticePage = () => {
    const { collectionId, vocabularyId } = practiceRouteApi.useParams();

    const {
        data: vocabulary,
        isLoading: isVocabLoading,
        isError: isVocabError,
        refetch: refetchVocab,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const {
        data: entries,
        isLoading: isEntriesLoading,
        isError: isEntriesError,
        refetch: refetchEntries,
    } = useVocabularyEntriesQuery(collectionId, vocabularyId);

    const {
        queue,
        currentIndex,
        isFlipped,
        isComplete,
        initSession,
        flip,
        rate,
    } = usePracticeStore();

    useEffect(() => {
        if (entries) {
            initSession(entries);
        }
    }, [entries, initSession]);

    const handlePracticeAgain = () => {
        if (entries) {
            initSession(entries);
        }
    };

    const isLoading = isVocabLoading || isEntriesLoading;
    const isError = isVocabError || isEntriesError;
    const breadcrumbItems = [
        { label: "Collections", ...collectionsPath() },
        {
            label: vocabulary?.collectionName ?? "...",
            ...collectionDetailPath(collectionId),
        },
        {
            label: vocabulary?.name ?? "Vocabulary",
            ...vocabularyDetailPath(collectionId, vocabularyId),
        },
        { label: "Practice" },
    ];

    const renderContent = () => {
        if (isLoading) {
            return <ContentSkeleton variant="detail" />;
        }

        if (isError || !vocabulary) {
            const handleRetry = () => {
                if (isVocabError) void refetchVocab();
                if (isEntriesError) void refetchEntries();
            };

            return (
                <RetryOnError
                    title="Failed to Load Vocabulary"
                    description="Something went wrong while loading the practice session. Please try again."
                    onRetry={handleRetry}
                />
            );
        }

        if (!entries || entries.length === 0) {
            return (
                <>
                    <PageHeader
                        title={vocabulary.name}
                        description="Practice session"
                    />
                    <Box className={styles.practicePage}>
                        <EmptyState
                            icon={
                                <SchoolIcon
                                    sx={{ fontSize: 32, color: "primary.main" }}
                                />
                            }
                            title="No entries to practice"
                            description="Add entries to this vocabulary before starting a practice session."
                        />
                    </Box>
                </>
            );
        }

        const currentCard = queue[currentIndex];

        if (isComplete || !currentCard) {
            return (
                <>
                    <PageHeader
                        title={vocabulary.name}
                        description="Practice session complete"
                    />
                    <Box className={styles.practicePage}>
                        <Box className={styles.completionArea}>
                            <SchoolIcon
                                sx={{ fontSize: 64, color: "primary.main" }}
                            />
                            <Typography variant="h2">
                                Session Complete!
                            </Typography>
                            <Typography variant="body1" color="text.secondary">
                                You reviewed all cards in{" "}
                                <strong>{vocabulary.name}</strong>.
                            </Typography>
                            <Box className={styles.completionActions}>
                                <Button
                                    variant="contained"
                                    onClick={handlePracticeAgain}
                                >
                                    Practice Again
                                </Button>
                            </Box>
                        </Box>
                    </Box>
                </>
            );
        }

        const progress = `${currentIndex + 1} / ${queue.length}`;

        return (
            <>
                <PageHeader
                    title={vocabulary.name}
                    description={
                        currentCard.pass === 2
                            ? "Practice session - Pass 2"
                            : "Practice session"
                    }
                    actions={
                        <Typography
                            variant="body2"
                            color="text.secondary"
                            sx={{ whiteSpace: "nowrap" }}
                        >
                            {progress}
                        </Typography>
                    }
                />
                <Box className={styles.practicePage}>
                    <Box className={styles.cardArea}>
                        <FlashCard
                            entry={currentCard.entry}
                            isFlipped={isFlipped}
                            onFlip={flip}
                        />

                        {isFlipped && (
                            <Box className={styles.ratingRow}>
                                <Button
                                    variant="contained"
                                    color="success"
                                    onClick={() => rate("easy")}
                                >
                                    Easy
                                </Button>
                                <Button
                                    variant="contained"
                                    color="warning"
                                    onClick={() => rate("hard")}
                                >
                                    Hard
                                </Button>
                                <Button
                                    variant="contained"
                                    color="error"
                                    onClick={() => rate("needsWork")}
                                >
                                    Needs Work
                                </Button>
                            </Box>
                        )}
                    </Box>
                </Box>
            </>
        );
    };

    return (
        <PageContainer>
            <TopBarBreadcrumbs items={breadcrumbItems} />
            {renderContent()}
        </PageContainer>
    );
};
