import { ApiError } from "../api/authApi";

export function parseApiError(error: ApiError): string[] {
    const messages: string[] = [];

    if (error.errors) {
        Object.values(error.errors).forEach((errorMessages) => {
            messages.push(...errorMessages);
        });
    }

    return messages;
}
