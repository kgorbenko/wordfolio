import { useState } from "react";
import {
    createFileRoute,
    Outlet,
    redirect,
    useNavigate,
} from "@tanstack/react-router";
import { Box, Fab, useMediaQuery, useTheme, Toolbar } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";

import { useAuthStore } from "../stores/authStore";
import { useUiStore } from "../stores/uiStore";
import { WordEntrySheet } from "../components/word-entry/WordEntrySheet";
import { Sidebar } from "../components/layouts/Sidebar";
import { TopBar } from "../components/layouts/TopBar";

const AuthenticatedLayout = () => {
    const theme = useTheme();
    const navigate = useNavigate();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const { openWordEntry, isWordEntryOpen, closeWordEntry } = useUiStore();
    const { clearAuth } = useAuthStore();

    const [sidebarOpen, setSidebarOpen] = useState(false);

    const handleLogout = () => {
        clearAuth();
        void navigate({ to: "/login" });
    };

    return (
        <Box
            sx={{
                minHeight: "100vh",
                bgcolor: "background.default",
                display: "flex",
                flexDirection: "column",
            }}
        >
            <TopBar
                onMenuClick={() => setSidebarOpen(true)}
                onLogout={handleLogout}
                showMenuButton={isMobile}
            />

            <Box sx={{ display: "flex", flex: 1 }}>
                {isMobile ? (
                    <Sidebar
                        variant="temporary"
                        open={sidebarOpen}
                        onClose={() => setSidebarOpen(false)}
                    />
                ) : (
                    <Sidebar variant="permanent" />
                )}

                <Box
                    component="main"
                    sx={{
                        flex: 1,
                        minWidth: 0,
                    }}
                >
                    <Toolbar />
                    <Outlet />
                </Box>
            </Box>

            <Fab
                color="primary"
                aria-label="Add word"
                onClick={() => openWordEntry()}
                sx={{
                    position: "fixed",
                    bottom: 24,
                    right: 24,
                    width: 56,
                    height: 56,
                }}
            >
                <AddIcon />
            </Fab>

            <WordEntrySheet open={isWordEntryOpen} onClose={closeWordEntry} />
        </Box>
    );
};

export const Route = createFileRoute("/_authenticated")({
    beforeLoad: async () => {
        const { isAuthenticated } = useAuthStore.getState();
        if (!isAuthenticated) {
            throw redirect({
                to: "/login",
            });
        }
    },
    component: AuthenticatedLayout,
});
