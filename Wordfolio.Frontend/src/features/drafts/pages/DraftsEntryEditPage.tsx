import { useCallback } from "react";
import { useNavigate } from "@tanstack/react-router";

import {
    draftsEntryEditRouteApi,
    draftsPath,
    draftsEntryDetailPath,
} from "../../../routes/_authenticated/drafts/routes";
import { PageContainer } from "../../../components/common/PageContainer";
import { PageHeader } from "../../../components/common/PageHeader";
import { BreadcrumbNav } from "../../../components/common/BreadcrumbNav";
import { RetryOnError } from "../../../components/common/RetryOnError";
import { ContentSkeleton } from "../../../components/common/ContentSkeleton";
import { useNotificationContext } from "../../../contexts/NotificationContext";

import { useDraftEntryQuery } from "../hooks/useDraftEntryQuery";
import { useUpdateDraftEntryMutation } from "../hooks/useUpdateDraftEntryMutation";
import { EntryForm } from "../../entries/components/EntryForm";
import { EntryFormValues, EntryFormOutput, Entry } from "../../entries/types";

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

    const numericEntryId = Number(entryId);

    const {
        data: entry,
        isLoading: isEntryLoading,
        isError: isEntryError,
        refetch: refetchEntry,
    } = useDraftEntryQuery(numericEntryId);

    const updateMutation = useUpdateDraftEntryMutation({
        onSuccess: () => {
            void navigate(draftsEntryDetailPath(numericEntryId));
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
        void navigate(draftsEntryDetailPath(numericEntryId));
    }, [navigate, numericEntryId]);

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
                        ...draftsEntryDetailPath(numericEntryId),
                    },
                    { label: "Edit" },
                ]}
            />
            <PageHeader title="Edit Entry" />
            {renderContent()}
        </PageContainer>
    );
};
