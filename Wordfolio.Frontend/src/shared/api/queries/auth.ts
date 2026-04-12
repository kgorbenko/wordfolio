import { useQuery, type UseQueryOptions } from "@tanstack/react-query";

import { client } from "../client";
import { mapPasswordRequirements, mapUserInfo } from "../mappers/auth";
import type { PasswordRequirements, UserInfo } from "../types/auth";

export const usePasswordRequirementsQuery = (
    options?: Partial<UseQueryOptions<PasswordRequirements>>
) =>
    useQuery({
        queryKey: ["password-requirements"],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/auth/password-requirements"
            );
            if (error) throw error;
            return mapPasswordRequirements(data!);
        },
        staleTime: Infinity,
        ...options,
    });

export const useUserInfoQuery = (
    options?: Partial<UseQueryOptions<UserInfo>>
) =>
    useQuery({
        queryKey: ["user-info"],
        queryFn: async () => {
            const { data, error } = await client.GET("/auth/manage/info");
            if (error) throw error;
            return mapUserInfo(data!);
        },
        staleTime: Infinity,
        ...options,
    });
