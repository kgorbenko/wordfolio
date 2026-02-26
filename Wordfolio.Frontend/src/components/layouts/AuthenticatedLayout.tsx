import { useState, useCallback, useRef } from "react";
import { Outlet, useNavigate } from "@tanstack/react-router";
import { Box, useMediaQuery, useTheme, Toolbar } from "@mui/material";

import { useAuthStore } from "../../stores/authStore";
import { useUiStore } from "../../stores/uiStore";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../features/entries/hooks/useDuplicateEntryDialog";
import { useCreateEntryMutation } from "../../features/entries/hooks/useCreateEntryMutation";
import { useCreateDraftMutation } from "../../features/drafts/hooks/useCreateDraftMutation";
import { CreateEntryRequest } from "../../features/entries/api/entriesApi";
import { VocabularyContext } from "../../features/entries/components/EntryLookupForm";
import { WordEntrySheet } from "../../features/entries";
import { loginPath } from "../../features/auth/routes";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import { assertNonNullable } from "../../utils/misc.ts";

import styles from "./AuthenticatedLayout.module.scss";

interface PendingEntry {
    readonly context: VocabularyContext | null;
    readonly request: CreateEntryRequest;
}

export const AuthenticatedLayout = () => {
    const theme = useTheme();
    const navigate = useNavigate();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const { openWordEntry, isWordEntryOpen, closeWordEntry } = useUiStore();
    const { clearAuth } = useAuthStore();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();
    const { raiseDuplicateEntryDialogAsync, dialogElement } =
        useDuplicateEntryDialog();

    const [sidebarOpen, setSidebarOpen] = useState(false);
    const pendingRequestRef = useRef<PendingEntry | null>(null);

    const createEntryMutation = useCreateEntryMutation({
        onSuccess: () => {
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
                const { context, request } = pendingRequestRef.current;
                assertNonNullable(context);
                createEntryMutation.mutate({
                    collectionId: context.collectionId,
                    vocabularyId: context.vocabularyId,
                    request: { ...request, allowDuplicate: true },
                });
            }
        },
    });

    const createDraftMutation = useCreateDraftMutation({
        onSuccess: () => {
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
                    ...pendingRequestRef.current.request,
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
        (context: VocabularyContext | null, request: CreateEntryRequest) => {
            pendingRequestRef.current = { context, request };
            if (context === null) {
                createDraftMutation.mutate(request);
            } else {
                createEntryMutation.mutate({
                    collectionId: context.collectionId,
                    vocabularyId: context.vocabularyId,
                    request,
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

    return (
        <Box className={styles.root}>
            <TopBar
                onMenuClick={() => setSidebarOpen(true)}
                onLogout={handleLogout}
                showMenuButton={isMobile}
            />

            <Box className={styles.contentWrapper}>
                {isMobile ? (
                    <Sidebar
                        variant="temporary"
                        open={sidebarOpen}
                        onClose={() => setSidebarOpen(false)}
                        onAddEntry={() => openWordEntry()}
                    />
                ) : (
                    <Sidebar
                        variant="permanent"
                        onAddEntry={() => openWordEntry()}
                    />
                )}

                <Box component="main" className={styles.main}>
                    <Toolbar />
                    <Outlet />
                </Box>
            </Box>

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
        </Box>
    );
};
