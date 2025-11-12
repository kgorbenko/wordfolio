import { createRootRoute, Outlet } from "@tanstack/react-router";
import { Box, CircularProgress } from "@mui/material";

import { useTokenRefresh } from "../hooks/useTokenRefresh";

const RootComponent = () => {
    const { isInitialRefreshing, hasAttemptedInitialRefresh } =
        useTokenRefresh();

    if (isInitialRefreshing || !hasAttemptedInitialRefresh) {
        return (
            <Box
                sx={{
                    display: "flex",
                    justifyContent: "center",
                    alignItems: "center",
                    height: "100vh",
                }}
            >
                <CircularProgress />
            </Box>
        );
    }

    return <Outlet />;
};

export const Route = createRootRoute({
    component: RootComponent,
});
