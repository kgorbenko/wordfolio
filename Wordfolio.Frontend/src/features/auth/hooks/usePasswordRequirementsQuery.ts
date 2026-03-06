import { useQuery } from "@tanstack/react-query";

import { authApi } from "../api/authApi";
import { mapPasswordRequirements } from "../api/mappers";

export function usePasswordRequirementsQuery() {
    return useQuery({
        queryKey: ["password-requirements"],
        queryFn: async () => {
            const response = await authApi.getPasswordRequirements();
            return mapPasswordRequirements(response);
        },
        staleTime: Infinity,
    });
}
