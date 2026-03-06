import { useState, useCallback, useRef } from "react";
import { Outlet, useNavigate } from "@tanstack/react-router";
import { Box, useMediaQuery, useTheme, Toolbar } from "@mui/material";

import { useAuthStore } from "../../stores/authStore";
import { useUiStore } from "../../stores/uiStore";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useDuplicateEntryDialog } from "../../hooks/useDuplicateEntryDialog";
import { useCreateEntryMutation } from "../../queries/useCreateEntryMutation";
import { useCreateDraftMutation } from "../../queries/useCreateDraftMutation";
import type { CreateEntryData } from "../../types/entries";
import { VocabularyContext } from "../entries/EntryLookupForm";
import { WordEntrySheet } from "../entries/WordEntrySheet";
import { loginPath } from "../../../features/auth/routes";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import { assertNonNullable } from "../../utils/misc.ts";

import styles from "./AuthenticatedLayout.module.scss";

interface PendingEntry {
    readonly context: VocabularyContext | null;
    readonly input: CreateEntryData;
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
