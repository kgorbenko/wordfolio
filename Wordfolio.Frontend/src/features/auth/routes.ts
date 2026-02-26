import { getRouteApi } from "@tanstack/react-router";

export const authRouteIds = {
    login: "/login",
    register: "/register",
    home: "/",
    dashboard: "/_authenticated/dashboard",
} as const;

export const loginRouteApi = getRouteApi(authRouteIds.login);

export function loginPath() {
    return { to: "/login" as const };
}

export function registerPath() {
    return { to: "/register" as const };
}

export function homePath() {
    return { to: "/" as const };
}

export function dashboardPath() {
    return { to: "/dashboard" as const };
}
