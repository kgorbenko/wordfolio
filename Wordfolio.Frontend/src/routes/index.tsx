import { createFileRoute, redirect } from "@tanstack/react-router";

import { useAuthStore } from "../shared/stores/authStore";
import { dashboardPath } from "../features/auth/routes";
import { ApertureLandingPage } from "../features/landing/pages/ApertureLandingPage";

export const Route = createFileRoute("/")({
    beforeLoad: () => {
        const { isAuthenticated } = useAuthStore.getState();
        if (isAuthenticated) {
            throw redirect(dashboardPath());
        }
    },
    component: ApertureLandingPage,
});
