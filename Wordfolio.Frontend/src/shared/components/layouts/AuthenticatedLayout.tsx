import { useState, useCallback, useRef } from "react";
import { Outlet, useNavigate, useParams } from "@tanstack/react-router";

import { useAuthStore } from "../../stores/authStore";
import { useUiStore } from "../../stores/uiStore";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../hooks/useDuplicateEntryDialog";
import { useCreateEntryMutation } from "../../queries/useCreateEntryMutation";
import { useCreateDraftMutation } from "../../queries/useCreateDraftMutation";
import { useCollectionsHierarchyQuery } from "../../queries/useCollectionsHierarchyQuery";
import type { CreateEntryData } from "../../types/entries";
import { VocabularyContext } from "../entries/EntryLookupForm";
import { WordEntrySheet } from "../entries/WordEntrySheet";
import { loginPath } from "../../../features/auth/routes";
import { collectionDetailPath } from "../../../features/collections/routes";
import { vocabularyDetailPath } from "../../../features/vocabularies/routes";
import { draftsPath } from "../../../features/drafts/routes";
import { AppLayout } from "./AppLayout";
import type { NavCollection, NavUser } from "./AppSidebar";
import { assertNonNullable } from "../../utils/misc";

interface PendingEntry {
    readonly context: VocabularyContext | null;
    readonly input: CreateEntryData;
}

export const AuthenticatedLayout = () => {
    const navigate = useNavigate();
    const params = useParams({ strict: false });
    const { openWordEntry, isWordEntryOpen, closeWordEntry } = useUiStore();
    const { clearAuth } = useAuthStore();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();
    const { data } = useCollectionsHierarchyQuery();

    const activeCollectionId = params.collectionId
        ? Number(params.collectionId)
        : undefined;
    const activeVocabularyId = params.vocabularyId
        ? Number(params.vocabularyId)
        : undefined;

    const [expandedCollections, setExpandedCollections] = useState<Set<number>>(
        () => new Set(activeCollectionId ? [activeCollectionId] : [])
    );
    const pendingRequestRef = useRef<PendingEntry | null>(null);

    const toggleCollection = (id: number) => {
        setExpandedCollections((prev) => {
            const next = new Set(prev);
            if (next.has(id)) next.delete(id);
            else next.add(id);
            return next;
        });
    };

    const createEntryMutation = useCreateEntryMutation({
        onSuccess: async () => {
            openSuccessNotification({ message: "Added to vocabulary" });
            closeWordEntry();
        },
        onError: () => {
            openErrorNotification({ message: "Failed to save entry" });
        },
        onDuplicateEntry: async (existingEntry) => {
            const addAnyway =
                await raiseDuplicateEntryDialogAsync(existingEntry);
            if (addAnyway && pendingRequestRef.current) {
                const { context, input } = pendingRequestRef.current;
                assertNonNullable(context);
                createEntryMutation.mutate({
                    collectionId: context.collectionId,
                    vocabularyId: context.vocabularyId,
                    input,
                    allowDuplicate: true,
                });
            }
        },
    });

    const createDraftMutation = useCreateDraftMutation({
        onSuccess: async () => {
            openSuccessNotification({ message: "Added to drafts" });
            closeWordEntry();
        },
        onError: () => {
            openErrorNotification({ message: "Failed to save entry" });
        },
        onDuplicateEntry: async (existingEntry) => {
            const addAnyway =
                await raiseDuplicateEntryDialogAsync(existingEntry);
            if (addAnyway && pendingRequestRef.current) {
                createDraftMutation.mutate({
                    input: pendingRequestRef.current.input,
                    allowDuplicate: true,
                });
            }
        },
    });

    const handleLogout = () => {
        clearAuth();
        void navigate(loginPath());
    };

    const handleSaveEntry = useCallback(
        (context: VocabularyContext | null, input: CreateEntryData) => {
            pendingRequestRef.current = { context, input };
            if (context === null) {
                createDraftMutation.mutate({ input });
            } else {
                createEntryMutation.mutate({
                    collectionId: context.collectionId,
                    vocabularyId: context.vocabularyId,
                    input,
                });
            }
        },
        [createEntryMutation, createDraftMutation]
    );

    const handleLookupError = useCallback(
        (message: string) => {
            openErrorNotification({ message });
        },
        [openErrorNotification]
    );

    const collections: NavCollection[] = (data?.collections ?? []).map((c) => ({
        id: c.id,
        name: c.name,
        entryCount: c.vocabularies.reduce((sum, v) => sum + v.entryCount, 0),
        active: c.id === activeCollectionId && activeVocabularyId === undefined,
        expanded: expandedCollections.has(c.id),
        activeChildId: activeVocabularyId,
        children: c.vocabularies.map((v) => ({
            id: v.id,
            name: v.name,
            entryCount: v.entryCount,
        })),
        onClick: () => void navigate(collectionDetailPath(c.id)),
        onExpand: () => toggleCollection(c.id),
        onChildClick: (vocabId) =>
            void navigate(vocabularyDetailPath(c.id, vocabId)),
    }));

    const user: NavUser = {
        initials: "U",
        email: "user@wordfolio.app",
        onLogout: handleLogout,
    };

    const draftCount = data?.defaultVocabulary?.entryCount ?? 0;

    return (
        <AppLayout
            draftCount={draftCount}
            collections={collections}
            user={user}
            onAddEntry={() => openWordEntry()}
            onDraftsClick={() => void navigate(draftsPath())}
        >
            <Outlet />
            <WordEntrySheet
                open={isWordEntryOpen}
                onClose={closeWordEntry}
                isSaving={
                    createEntryMutation.isPending ||
                    createDraftMutation.isPending
                }
                onSave={handleSaveEntry}
                onLookupError={handleLookupError}
            />
            {dialogElement}
        </AppLayout>
    );
};
