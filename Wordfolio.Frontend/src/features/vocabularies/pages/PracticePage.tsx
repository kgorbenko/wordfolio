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

    if (isLoading) {
        return (
            <PageContainer>
                <TopBarBreadcrumbs items={breadcrumbItems} />
                <ContentSkeleton variant="detail" />
            </PageContainer>
        );
    }

    if (isError) {
        const handleRetry = () => {
            if (isVocabError) void refetchVocab();
            if (isEntriesError) void refetchEntries();
        };

        return (
            <PageContainer>
                <TopBarBreadcrumbs items={breadcrumbItems} />
                <RetryOnError
                    title="Failed to Load Vocabulary"
                    description="Something went wrong while loading the practice session. Please try again."
                    onRetry={handleRetry}
                />
            </PageContainer>
        );
    }

    if (!entries || entries.length === 0) {
        return (
            <PageContainer>
                <TopBarBreadcrumbs items={breadcrumbItems} />
                <Box className={styles.page}>
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
            </PageContainer>
        );
    }

    const currentCard = queue[currentIndex];

    if (isComplete || !currentCard) {
        return (
            <PageContainer>
                <TopBarBreadcrumbs items={breadcrumbItems} />
                <Box className={styles.page}>
                    <Box className={styles.completionArea}>
                        <SchoolIcon
                            sx={{ fontSize: 64, color: "primary.main" }}
                        />
                        <Typography variant="h2">Session Complete!</Typography>
                        <Typography variant="body1" color="text.secondary">
                            You reviewed all cards in{" "}
                            <strong>
                                {vocabulary?.name ?? "this vocabulary"}
                            </strong>
                            .
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
            </PageContainer>
        );
    }

    const progress = `${currentIndex + 1} / ${queue.length}`;

    return (
        <PageContainer>
            <TopBarBreadcrumbs items={breadcrumbItems} />
            <Box className={styles.page}>
                <Box className={styles.header}>
                    <Box className={styles.headerTitle}>
                        <Typography variant="h4">
                            {vocabulary?.name ?? "Practice"}
                        </Typography>
                        <Box className={styles.headerMeta}>
                            <Typography variant="body2" color="text.secondary">
                                Practice session
                            </Typography>
                            {currentCard.pass === 2 && (
                                <Typography
                                    variant="body2"
                                    color="text.secondary"
                                >
                                    Pass 2
                                </Typography>
                            )}
                        </Box>
                    </Box>
                    <Typography variant="body2" className={styles.progress}>
                        {progress}
                    </Typography>
                </Box>

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
        </PageContainer>
    );
};
