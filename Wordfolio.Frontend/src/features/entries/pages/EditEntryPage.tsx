import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import { entryEditRouteApi, entryDetailPath } from "../routes";
import {
    collectionsPath,
    collectionDetailPath,
} from "../../collections/routes";
import { vocabularyDetailPath } from "../../vocabularies/routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";

import { useVocabularyDetailQuery } from "../../../shared/queries/useVocabularyDetailQuery";
import { useEntryQuery } from "../hooks/useEntryQuery";
import { useUpdateEntryMutation } from "../hooks/useUpdateEntryMutation";
import { EntryForm } from "../../../shared/components/entries/EntryForm";
import type {
    Entry,
    EntryFormValues,
    CreateEntryData,
} from "../../../shared/types/entries";

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

    const {
        data: vocabulary,
        isLoading: isVocabularyLoading,
        isError: isVocabularyError,
        refetch: refetchVocabulary,
    } = useVocabularyDetailQuery(collectionId, vocabularyId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useEntryQuery(collectionId, vocabularyId, entryId);

    const updateMutation = useUpdateEntryMutation({
        onSuccess: async () => {
            await navigate(
                entryDetailPath(collectionId, vocabularyId, entryId)
            );
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to update entry. Please try again.",
            });
        },
    });

    const handleSubmit = useCallback(
        (data: CreateEntryData) => {
            updateMutation.mutate({
                collectionId,
                vocabularyId,
                entryId,
                data,
            });
        },
        [updateMutation, collectionId, vocabularyId, entryId]
    );

    const handleCancel = useCallback(() => {
        void navigate(entryDetailPath(collectionId, vocabularyId, entryId));
    }, [collectionId, entryId, navigate, vocabularyId]);

    const isLoading = isVocabularyLoading || isEntryLoading;
    const isError = isVocabularyError || isEntryError;

    const renderContent = () => {
        if (isLoading) return <ContentSkeleton variant="form" />;

        if (isError || !vocabulary || !entry) {
            const handleRetry = () => {
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
    };

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Collections", ...collectionsPath() },
                    {
                        label: vocabulary?.collectionName ?? "...",
                        ...collectionDetailPath(collectionId),
                    },
                    {
                        label: vocabulary?.name ?? "...",
                        ...vocabularyDetailPath(collectionId, vocabularyId),
                    },
                    {
                        label: entry?.entryText ?? "...",
                        ...entryDetailPath(collectionId, vocabularyId, entryId),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Entry" />
            {renderContent()}
        </PageContainer>
    );
};
