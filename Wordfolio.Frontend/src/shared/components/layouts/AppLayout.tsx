import { useState, useCallback } from "react";
import { Box, useMediaQuery, useTheme } from "@mui/material";

import { BreadcrumbPortalContext } from "../../contexts/BreadcrumbPortalContext";
import { AppSidebar } from "./AppSidebar";
import { AppTopBar } from "./AppTopBar";
import type { NavCollection, NavUser } from "./AppSidebar";

import styles from "./AppLayout.module.scss";

interface AppLayoutProps {
    readonly children: React.ReactNode;
    readonly draftCount: number;
    readonly collections: NavCollection[];
    readonly user: NavUser;
    readonly onAddEntry: () => void;
    readonly onDraftsClick: () => void;
}

export const AppLayout = ({
    children,
    draftCount,
    collections,
    user,
    onAddEntry,
    onDraftsClick,
}: AppLayoutProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const [sidebarOpen, setSidebarOpen] = useState(false);
    const [portalNode, setPortalNode] = useState<HTMLDivElement | null>(null);

    const portalRef = useCallback((node: HTMLDivElement | null) => {
        setPortalNode(node);
    }, []);

    const sidebarProps = {
        draftCount,
        collections,
        user,
        onAddEntry,
        onDraftsClick,
    };

    return (
        <BreadcrumbPortalContext.Provider value={portalNode}>
            <Box className={styles.root} sx={{ bgcolor: "background.sidebar" }}>
                {isMobile ? (
                    <AppSidebar
                        variant="temporary"
                        open={sidebarOpen}
                        onClose={() => setSidebarOpen(false)}
                        {...sidebarProps}
                    />
                ) : (
                    <AppSidebar variant="permanent" {...sidebarProps} />
                )}

                <Box className={styles.content}>
                    <AppTopBar
                        showMenuButton={isMobile}
                        onMenuClick={() => setSidebarOpen(true)}
                        portalRef={portalRef}
                    />
                    <main className={styles.main}>{children}</main>
                </Box>
            </Box>
        </BreadcrumbPortalContext.Provider>
    );
};
