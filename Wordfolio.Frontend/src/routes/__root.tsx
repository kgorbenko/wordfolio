import { createRootRoute, Outlet } from "@tanstack/react-router";
import { CircularProgress } from "@mui/material";

import { useTokenRefresh } from "../hooks/useTokenRefresh";

const RootComponent = () => {
    const { isInitializing } = useTokenRefresh();

    if (isInitializing) {
        return (
            <div className="centered-page-container">
                <CircularProgress />
            </div>
        );
    }

    return <Outlet />;
};

export const Route = createRootRoute({
    component: RootComponent,
});
