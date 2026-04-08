import { createFileRoute, redirect } from "@tanstack/react-router";

import { useAuthStore } from "../shared/stores/authStore";
import { AuthenticatedLayout } from "../shared/components/layouts/AuthenticatedLayout";
import { loginPath } from "../features/auth/routes";

export const Route = createFileRoute("/_authenticated")({
    beforeLoad: async ({ location }) => {
        const { isAuthenticated } = useAuthStore.getState();
        if (!isAuthenticated) {
            throw redirect(loginPath({ redirect: location.href }));
        }
    },
    component: AuthenticatedLayout,
});
