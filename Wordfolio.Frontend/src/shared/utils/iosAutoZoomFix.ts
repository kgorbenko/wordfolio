import { ensureNonNullable } from "./misc";

export function preventIosFocusAutoZoom(): void {
    const userAgent = window.navigator.userAgent;
    const isIos = /iPad|iPhone|iPod/.test(userAgent);
    const isIpadOs =
        /Macintosh/.test(userAgent) && window.navigator.maxTouchPoints > 1;

    if (!isIos && !isIpadOs) {
        return;
    }

    const viewportMeta = ensureNonNullable(
        document.querySelector<HTMLMetaElement>('meta[name="viewport"]')
    );
    const existingContent = viewportMeta.getAttribute("content") ?? "";
    const preservedParts = existingContent
        .split(",")
        .map((part) => part.trim())
        .filter((part) => part.length > 0 && !/^maximum-scale\s*=/.test(part));
    preservedParts.push("maximum-scale=1");
    viewportMeta.setAttribute("content", preservedParts.join(", "));
}
