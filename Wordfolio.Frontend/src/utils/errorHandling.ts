import { ApiError } from "../api/authApi";

export interface ParsedError {
    readonly fieldErrors: Record<string, string>;
    readonly generalError?: string;
}

export function parseApiError(
    error: ApiError,
    validFieldNames: readonly string[],
    fallbackMessage: string
): ParsedError {
    const fieldErrors: Record<string, string> = {};
    let hasUnmappedError = false;
    const unmappedMessages: string[] = [];

    if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]) => {
            const fieldName = field.toLowerCase();
            if (validFieldNames.includes(fieldName)) {
                fieldErrors[fieldName] = messages.join(", ");
            } else {
                hasUnmappedError = true;
                unmappedMessages.push(...messages);
            }
        });
    }

    const generalError = hasUnmappedError
        ? unmappedMessages.join(" ")
        : error.errors
            ? undefined
            : fallbackMessage;

    return { fieldErrors, generalError };
}
