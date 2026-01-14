import { useQuery } from "@tanstack/react-query";
import { vocabulariesApi } from "../api/vocabulariesApi";

export function useCollectionsHierarchyQuery() {
    return useQuery({
        queryKey: ["collections-hierarchy"],
        queryFn: () => vocabulariesApi.getCollectionsHierarchy(),
    });
}
