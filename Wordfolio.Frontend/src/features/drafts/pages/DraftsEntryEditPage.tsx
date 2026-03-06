import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import {
    draftsEntryEditRouteApi,
    draftsPath,
    draftsEntryDetailPath,
} from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { RetryOnError } from "../../../shared/components/RetryOnError";
import { ContentSkeleton } from "../../../shared/components/ContentSkeleton";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";

import { useDraftEntryQuery } from "../hooks/useDraftEntryQuery";
import { useUpdateDraftEntryMutation } from "../hooks/useUpdateDraftEntryMutation";
import type {
    Entry,
    EntryFormValues,
    CreateEntryData,
} from "../../../shared/types/entries";
import { EntryForm } from "../../../shared/components/entries/EntryForm";

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

export const DraftsEntryEditPage = () => {
    const { entryId } = draftsEntryEditRouteApi.useParams();
    const navigate = useNavigate();
    const { openErrorNotification } = useNotificationContext();

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useDraftEntryQuery(entryId);

    const updateMutation = useUpdateDraftEntryMutation({
        onSuccess: () => {
            void navigate(draftsEntryDetailPath(entryId));
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
                entryId,
                data,
            });
        },
        [updateMutation, entryId]
    );

    const handleCancel = useCallback(() => {
        void navigate(draftsEntryDetailPath(entryId));
    }, [navigate, entryId]);

    const isLoading = isEntryLoading;
    const isError = isEntryError;

    const renderContent = useCallback(() => {
        if (isLoading) return <ContentSkeleton variant="form" />;

        if (isError || !entry) {
            const handleRetry = () => {
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
        entry,
        isEntryError,
        refetchEntry,
        handleSubmit,
        handleCancel,
        updateMutation.isPending,
    ]);

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Drafts", ...draftsPath() },
                    {
                        label: entry?.entryText ?? "...",
                        ...draftsEntryDetailPath(entryId),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Entry" />
            {renderContent()}
        </PageContainer>
    );
};
