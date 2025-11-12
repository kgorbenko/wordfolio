import { useMutation } from "@tanstack/react-query";

import {
    authApi,
    RefreshRequest,
    RefreshResponse,
    ApiError,
} from "../api/authApi";

interface UseRefreshMutationOptions {
    onSuccess?: (data: RefreshResponse) => void;
    onError?: (error: ApiError) => void;
}

export const useRefreshMutation = (options?: UseRefreshMutationOptions) => {
    return useMutation({
        mutationFn: (request: RefreshRequest) => authApi.refresh(request),
        onSuccess: options?.onSuccess,
        onError: options?.onError,
    });
};
