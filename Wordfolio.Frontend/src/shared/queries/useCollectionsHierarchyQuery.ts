import { useQuery } from "@tanstack/react-query";

import { collectionsHierarchyApi } from "../api/collectionsHierarchy.ts";
import { mapCollectionsHierarchy } from "../api/collectionsHierarchyMappers";
import type { CollectionsHierarchy } from "../types/collectionsHierarchy";

export function useCollectionsHierarchyQuery() {
    return useQuery<CollectionsHierarchy>({
        queryKey: ["collections-hierarchy"],
        queryFn: async () => {
            const response =
                await collectionsHierarchyApi.getCollectionsHierarchy();
            return mapCollectionsHierarchy(response);
        },
    });
}
