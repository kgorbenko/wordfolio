import { useState, useCallback } from "react";
import { Outlet, useNavigate } from "@tanstack/react-router";
import { Box, useMediaQuery, useTheme, Toolbar } from "@mui/material";

import { useAuthStore } from "../../stores/authStore";
import { useUiStore } from "../../stores/uiStore";
import { useNotificationContext } from "../../contexts/NotificationContext";
import { useCreateEntryMutation } from "../../features/entries/hooks/useCreateEntryMutation";
import { CreateEntryRequest } from "../../features/entries/api/entriesApi";
import { WordEntrySheet } from "../../features/entries";
import { Sidebar } from "./Sidebar";
import { TopBar } from "./TopBar";
import styles from "./AuthenticatedLayout.module.scss";

export const AuthenticatedLayout = () => {
    const theme = useTheme();
    const navigate = useNavigate();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const { openWordEntry, isWordEntryOpen, closeWordEntry } = useUiStore();
    const { clearAuth } = useAuthStore();
    const { openSuccessNotification, openErrorNotification } =
        useNotificationContext();

    const [sidebarOpen, setSidebarOpen] = useState(false);

    const createEntryMutation = useCreateEntryMutation({
        onSuccess: () => {
            openSuccessNotification({ message: "Added to vocabulary" });
            closeWordEntry();
        },
        onError: () => {
            openErrorNotification({ message: "Failed to save entry" });
        },
    });

    const handleLogout = () => {
        clearAuth();
        void navigate({ to: "/login" });
    };

    const handleSaveEntry = useCallback(
        (request: CreateEntryRequest) => {
            createEntryMutation.mutate(request);
        },
        [createEntryMutation]
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
                        onAddWord={() => openWordEntry()}
                    />
                ) : (
                    <Sidebar
                        variant="permanent"
                        onAddWord={() => openWordEntry()}
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
                isSaving={createEntryMutation.isPending}
                onSave={handleSaveEntry}
                onLookupError={handleLookupError}
            />
        </Box>
    );
};
