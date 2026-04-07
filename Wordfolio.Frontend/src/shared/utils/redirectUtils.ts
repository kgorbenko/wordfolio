const authPaths = ["/login", "/register"];

export function getSafeRedirectPath(
    redirectPath: string | undefined
): string | undefined;
export function getSafeRedirectPath(
    redirectPath: string | undefined,
    fallbackPath: string
): string;
export function getSafeRedirectPath(
    redirectPath: string | undefined,
    fallbackPath?: string
): string | undefined {
    if (!redirectPath) return fallbackPath;
    if (!redirectPath.startsWith("/")) return fallbackPath;
    if (redirectPath.startsWith("//")) return fallbackPath;

    const pathname = redirectPath.split("?")[0].split("#")[0];
    if (authPaths.includes(pathname)) return fallbackPath;

    return redirectPath;
}
