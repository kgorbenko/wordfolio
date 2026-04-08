import React from "react";
import ReactDOM from "react-dom/client";
import { RouterProvider, createRouter } from "@tanstack/react-router";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ThemeProvider } from "@mui/material/styles";
import CssBaseline from "@mui/material/CssBaseline";
import { NotificationProvider } from "./shared/contexts/NotificationProvider";
import { ConfirmDialogProvider } from "./shared/contexts/ConfirmDialogProvider";
import { preventIosFocusAutoZoom } from "./shared/utils/iosAutoZoomFix";
import { theme } from "./theme";
import { routeTree } from "./routeTree.gen";
import { LostInTranslationAtlasPage } from "./features/not-found/pages/LostInTranslationAtlasPage";

import "./main.css";

preventIosFocusAutoZoom();

const router = createRouter({
    routeTree,
    defaultNotFoundComponent: LostInTranslationAtlasPage,
});

declare module "@tanstack/react-router" {
    interface Register {
        router: typeof router;
    }
}

const queryClient = new QueryClient({
    defaultOptions: {
        queries: {
            retry: 1,
            refetchOnWindowFocus: false,
        },
    },
});

ReactDOM.createRoot(document.getElementById("root")!).render(
    <React.StrictMode>
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <NotificationProvider>
                <ConfirmDialogProvider>
                    <QueryClientProvider client={queryClient}>
                        <RouterProvider router={router} />
                    </QueryClientProvider>
                </ConfirmDialogProvider>
            </NotificationProvider>
        </ThemeProvider>
    </React.StrictMode>
);
