import { createFileRoute } from "@tanstack/react-router";

const DashboardPage = () => {
    return null;
};

export const Route = createFileRoute("/_authenticated/dashboard")({
    component: DashboardPage,
});
