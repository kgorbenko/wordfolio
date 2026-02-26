import { createFileRoute, redirect } from "@tanstack/react-router";

import { useAuthStore } from "../stores/authStore";
import { AuthenticatedLayout } from "../components/layouts/AuthenticatedLayout";
import { loginPath } from "../features/auth/routes";

export const Route = createFileRoute("/_authenticated")({
    beforeLoad: async () => {
        const { isAuthenticated } = useAuthStore.getState();
        if (!isAuthenticated) {
            throw redirect(loginPath());
        }
    },
    component: AuthenticatedLayout,
});
