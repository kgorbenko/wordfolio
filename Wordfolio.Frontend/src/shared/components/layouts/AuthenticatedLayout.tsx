import { useState, useCallback, useRef } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { Outlet, useNavigate, useParams } from "@tanstack/react-router";
import { CircularProgress } from "@mui/material";

import { useAuthStore } from "../../stores/authStore";
import { useUiStore } from "../../stores/uiStore";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../hooks/useDuplicateEntryDialog";
import { useCreateEntryMutation } from "../../api/mutations/entries";
import { useCreateDraftMutation } from "../../api/mutations/drafts";
import { useCollectionsHierarchyQuery } from "../../api/queries/collections";
import { useUserInfoQuery } from "../../../features/auth/hooks/useUserInfoQuery";
import type { CreateEntryData } from "../../api/types/entries";
import { VocabularyContext } from "../entries/EntryLookupForm";
import { WordEntrySheet } from "../entries/WordEntrySheet";
import { loginPath } from "../../../features/auth/routes";
import { collectionDetailPath } from "../../../features/collections/routes";
import { vocabularyDetailPath } from "../../../features/vocabularies/routes";
import { draftsPath } from "../../../features/drafts/routes";
import { RetryOnError } from "../RetryOnError";
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
    const queryClient = useQueryClient();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();
    const {
        data: hierarchyData,
        isLoading: isHierarchyLoading,
        isError: isHierarchyError,
        refetch: refetchHierarchy,
    } = useCollectionsHierarchyQuery();
    const {
        data: userInfoData,
        isLoading: isUserInfoLoading,
        isError: isUserInfoError,
        refetch: refetchUserInfo,
    } = useUserInfoQuery();

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
        queryClient.clear();
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

    if (isHierarchyLoading || isUserInfoLoading) {
        return (
            <div className="centered-page-container">
                <CircularProgress />
            </div>
        );
    }

    if (
        isHierarchyError ||
        isUserInfoError ||
        !hierarchyData ||
        !userInfoData
    ) {
        return (
            <div className="centered-page-container">
                <RetryOnError
                    title="Failed to Load Application"
                    description="Something went wrong while loading Wordfolio. Please try again."
                    onRetry={() => {
                        void refetchHierarchy();
                        void refetchUserInfo();
                    }}
                />
            </div>
        );
    }

    const collections: NavCollection[] = hierarchyData.collections.map((c) => ({
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

    const userEmail = userInfoData.email;

    const user: NavUser = {
        initials: userEmail[0].toUpperCase(),
        email: userEmail,
        onLogout: handleLogout,
    };

    const draftCount = hierarchyData.defaultVocabulary?.entryCount ?? 0;

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
