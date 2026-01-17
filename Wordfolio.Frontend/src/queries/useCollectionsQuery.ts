import { useQuery } from "@tanstack/react-query";

import { vocabulariesApi } from "../api/vocabulariesApi";

export function useCollectionsQuery() {
    return useQuery({
        queryKey: ["collections"],
        queryFn: () => vocabulariesApi.getCollections(),
    });
}
