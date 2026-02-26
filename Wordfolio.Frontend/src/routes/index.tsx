import { createFileRoute, redirect } from "@tanstack/react-router";

import { useAuthStore } from "../stores/authStore";
import { HomePage } from "../pages/HomePage";
import { dashboardPath } from "../features/auth/routes";

export const Route = createFileRoute("/")({
    beforeLoad: () => {
        const { isAuthenticated } = useAuthStore.getState();
        if (isAuthenticated) {
            throw redirect(dashboardPath());
        }
    },
    component: HomePage,
});
