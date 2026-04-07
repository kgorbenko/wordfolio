import { getRouteApi } from "@tanstack/react-router";

export const authRouteIds = {
    login: "/login",
    register: "/register",
    home: "/",
    dashboard: "/_authenticated/dashboard",
} as const;

export const loginRouteApi = getRouteApi(authRouteIds.login);
export const registerRouteApi = getRouteApi(authRouteIds.register);

interface AuthPathOptions {
    readonly redirect: string;
}

export function loginPath(): { to: "/login" };
export function loginPath(options: AuthPathOptions): {
    to: "/login";
    search: { redirect: string };
};
export function loginPath(options?: AuthPathOptions) {
    if (options !== undefined) {
        return {
            to: "/login" as const,
            search: { redirect: options.redirect },
        };
    }
    return { to: "/login" as const };
}

export function registerPath(): { to: "/register" };
export function registerPath(options: AuthPathOptions): {
    to: "/register";
    search: { redirect: string };
};
export function registerPath(options?: AuthPathOptions) {
    if (options !== undefined) {
        return {
            to: "/register" as const,
            search: { redirect: options.redirect },
        };
    }
    return { to: "/register" as const };
}

export function homePath() {
    return { to: "/" as const };
}

export function dashboardPath() {
    return { to: "/dashboard" as const };
}
