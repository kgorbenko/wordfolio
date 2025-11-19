import { ApiError } from "../api/authApi";

export function parseApiError(error: ApiError): string[] {
    return error.errors ? Object.values(error.errors).flatMap((x) => x) : [];
}
