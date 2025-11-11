import { useMutation } from '@tanstack/react-query';
import { authApi, LoginRequest, LoginResponse, ApiError } from '../api/authApi';

interface UseLoginMutationOptions {
    onSuccess?: (data: LoginResponse) => void;
    onError?: (error: ApiError) => void;
}

export function useLoginMutation(options?: UseLoginMutationOptions) {
    return useMutation({
        mutationFn: (request: LoginRequest) => authApi.login(request),
        onSuccess: options?.onSuccess,
        onError: options?.onError,
    });
}
