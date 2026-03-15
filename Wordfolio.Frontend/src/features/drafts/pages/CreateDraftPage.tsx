import { useCallback, useRef } from "react";
import { useNavigate } from "@tanstack/react-router";

import { draftsPath } from "../routes";
import { PageContainer } from "../../../shared/components/PageContainer";
import { PageHeader } from "../../../shared/components/PageHeader";
import { BreadcrumbNav } from "../../../shared/components/BreadcrumbNav";
import { useNotificationContext } from "../../../shared/contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../../shared/hooks/useDuplicateEntryDialog";
import { useCreateDraftMutation } from "../../../shared/queries/useCreateDraftMutation";
import type { CreateEntryData } from "../../../shared/types/entries";
import {
    EntryLookupForm,
    VocabularyContext,
} from "../../../shared/components/entries/EntryLookupForm";

export const CreateDraftPage = () => {
    const navigate = useNavigate();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();
    const pendingRequestRef = useRef<CreateEntryData | null>(null);

    const createMutation = useCreateDraftMutation({
        onSuccess: async () => {
            openSuccessNotification({ message: "Draft created successfully" });
            await navigate(draftsPath());
        },
        onError: () => {
            openErrorNotification({
                message: "Failed to create draft. Please try again.",
            });
        },
        onDuplicateEntry: async (existingEntry) => {
            const addAnyway =
                await raiseDuplicateEntryDialogAsync(existingEntry);
            if (addAnyway && pendingRequestRef.current) {
                createMutation.mutate({
                    input: pendingRequestRef.current,
                    allowDuplicate: true,
                });
            }
        },
    });

    const handleSave = useCallback(
        (_: VocabularyContext | null, request: CreateEntryData) => {
            pendingRequestRef.current = request;
            createMutation.mutate({ input: request });
        },
        [createMutation]
    );

    const handleCancel = useCallback(() => {
        void navigate(draftsPath());
    }, [navigate]);

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    return (
        <PageContainer>
            <BreadcrumbNav
                items={[
                    { label: "Drafts", ...draftsPath() },
                    { label: "New Draft" },
                ]}
            />
            <PageHeader title="Create Draft" />
            <EntryLookupForm
                showVocabularySelector={false}
                isSaving={createMutation.isPending}
                onSave={handleSave}
                onCancel={handleCancel}
                onLookupError={handleLookupError}
                autoFocus={true}
                variant="page"
            />
            {dialogElement}
        </PageContainer>
    );
};
