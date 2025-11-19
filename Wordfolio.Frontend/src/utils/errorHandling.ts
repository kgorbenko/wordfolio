import { ApiError } from "../api/authApi";

export interface ParsedError {
    readonly fieldErrors: Record<string, string[]>;
    readonly generalErrors: string[];
}

export function parseApiError(
    error: ApiError,
    validFieldNames: readonly string[]
): ParsedError {
    const fieldErrors: Record<string, string[]> = {};
    const unmappedMessages: string[] = [];

    if (error.errors) {
        Object.entries(error.errors).forEach(([field, messages]) => {
            const fieldName = field.toLowerCase();
            if (validFieldNames.includes(fieldName)) {
                fieldErrors[fieldName] = messages;
            } else {
                unmappedMessages.push(...messages);
            }
        });
    }

    return { fieldErrors, generalErrors: unmappedMessages };
}
