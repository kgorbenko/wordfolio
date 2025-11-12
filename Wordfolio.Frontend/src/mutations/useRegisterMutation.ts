import { useMutation } from "@tanstack/react-query";
import { authApi, RegisterRequest, ApiError } from "../api/authApi";

interface UseRegisterMutationOptions {
    onSuccess?: () => void;
    onError?: (error: ApiError) => void;
}

export function useRegisterMutation(options?: UseRegisterMutationOptions) {
    return useMutation({
        mutationFn: (request: RegisterRequest) => authApi.register(request),
        onSuccess: options?.onSuccess,
        onError: options?.onError,
    });
}
