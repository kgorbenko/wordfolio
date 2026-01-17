import { useQuery } from "@tanstack/react-query";

import { vocabulariesApi } from "../api/vocabulariesApi";

export function useCollectionQuery(id: number) {
    return useQuery({
        queryKey: ["collections", id],
        queryFn: () => vocabulariesApi.getCollection(id),
    });
}
