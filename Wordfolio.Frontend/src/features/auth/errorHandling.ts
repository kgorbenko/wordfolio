import { ApiError } from "../../shared/api/types/entries";

export function parseApiError(error: ApiError): string[] {
    return error.errors ? Object.values(error.errors).flatMap((x) => x) : [];
}
