import { createFileRoute, redirect } from "@tanstack/react-router";

import { useAuthStore } from "../stores/authStore";
import { HomePage } from "../pages/HomePage";

export const Route = createFileRoute("/")({
    beforeLoad: () => {
        const { isAuthenticated } = useAuthStore.getState();
        if (isAuthenticated) {
            throw redirect({
                to: "/dashboard",
            });
        }
    },
    component: HomePage,
});
