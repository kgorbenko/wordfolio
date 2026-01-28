import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { entryEditRouteApi } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { collectionsPath } from "../../../routes/_authenticated/collections/routes";
import { collectionDetailPath } from "../../../routes/_authenticated/collections/routes";
import { vocabularyDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/routes";
import { entryDetailPath } from "../../../routes/_authenticated/collections/$collectionId/vocabularies/$vocabularyId/entries/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";

import { useCollectionQuery } from "../../collections/hooks/useCollectionQuery";
import { useVocabularyQuery } from "../../vocabularies/hooks/useVocabularyQuery";
import { useEntryQuery } from "../hooks/useEntryQuery";
import { useUpdateEntryMutation } from "../hooks/useUpdateEntryMutation";
import { EntryForm } from "../components/EntryForm";
import { EntryFormValues, EntryFormOutput, Entry } from "../types";

const mapEntryToFormValues = (entry: Entry): EntryFormValues => ({
    entryText: entry.entryText,
    definitions: entry.definitions.map((def) => ({
        id: String(def.id),
        definitionText: def.definitionText,
        source: def.source,
        examples: def.examples.map((ex) => ({
            id: String(ex.id),
            exampleText: ex.exampleText,
            source: ex.source,
        })),
    })),
    translations: entry.translations.map((trans) => ({
        id: String(trans.id),
        translationText: trans.translationText,
        source: trans.source,
        examples: trans.examples.map((ex) => ({
            id: String(ex.id),
            exampleText: ex.exampleText,
            source: ex.source,
        })),
    })),
});

export const EditEntryPage = () => {
    const { collectionId, vocabularyId, entryId } =
        entryEditRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

    const numericCollectionId = Number(collectionId);
    const numericVocabularyId = Number(vocabularyId);
    const numericEntryId = Number(entryId);

    const {
        data: collection,
        isLoading: isCollectionLoading,
        isError: isCollectionError,
        refetch: refetchCollection,
    } = useCollectionQuery(numericCollectionId);

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyQuery(numericCollectionId, numericVocabularyId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(numericEntryId);

    const updateMutation = useUpdateEntryMutation({
        onSuccess: () => {
            void navigate(
                entryDetailPath(
                    numericCollectionId,
                    numericVocabularyId,
                    numericEntryId
                )
            );
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update entry. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: EntryFormOutput) => {
            updateMutation.mutate({
                entryId: numericEntryId,
                request: {
                    entryText: data.entryText,
                    definitions: data.definitions,
                    translations: data.translations,
                },
            });
        },
        [updateMutation, numericEntryId]
    );

    const handleCancel = useCallback(() => {
        void navigate(
            entryDetailPath(
                numericCollectionId,
                numericVocabularyId,
                numericEntryId
            )
        );
    }, [navigate, numericCollectionId, numericVocabularyId, numericEntryId]);

    const isLoading =
        isCollectionLoading || isVocabularyLoading || isEntryLoading;
    const isError = isCollectionError || isVocabularyError || isEntryError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="form" />;

        if (isError || !collection || !vocabulary || !entry) {
            const handleRetry = () => {
                if (isCollectionError) void refetchCollection();
                if (isVocabularyError) void refetchVocabulary();
                if (isEntryError) void refetchEntry();
            };

            return (
                <RetryOnError
                    title="Failed to Load Entry"
                    description="Something went wrong while loading this entry. Please try again."
                    onRetry={handleRetry}
                />
            );
        }

        return (
            <EntryForm
                defaultValues={mapEntryToFormValues(entry)}
                onSubmit={handleSubmit}
                onCancel={handleCancel}
                submitLabel="Save"
                isLoading={updateMutation.isPending}
            />
        );
    }, [
        isLoading,
        isError,
        collection,
        vocabulary,
        entry,
        isCollectionError,
        isVocabularyError,
        isEntryError,
        refetchCollection,
        refetchVocabulary,
        refetchEntry,
        handleSubmit,
        handleCancel,
        updateMutation.isPending,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: collection?.name ?? "...",
                        ...collectionDetailPath(numericCollectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(
                            numericCollectionId,
                            numericVocabularyId
                        ),
                    },
                    {
                        label: entry?.entryText ?? "...",
                        ...entryDetailPath(
                            numericCollectionId,
                            numericVocabularyId,
                            numericEntryId
                        ),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Entry" />
            {renderContent()}
        </PageContainer>
    );
};
