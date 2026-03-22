import { useQuery } from "@tanstack/react-query";

import { authApi } from "../api/authApi";
import { mapUserInfo } from "../api/mappers";

export function useUserInfoQuery() {
    return useQuery({
        queryKey: ["user-info"],
        queryFn: async () => {
            const response = await authApi.getUserInfo();
            return mapUserInfo(response);
        },
        staleTime: Infinity,
    });
}
