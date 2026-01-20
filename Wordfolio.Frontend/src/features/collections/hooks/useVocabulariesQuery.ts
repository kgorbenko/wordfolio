import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapVocabularies } from "../api/mappers";
import { Vocabulary } from "../types";

export const useVocabulariesQuery = (
    collectionId: number,
    options?: Partial<UseQueryOptions<Vocabulary[]>>
) =>
    useQuery({
        queryKey: ["vocabularies", collectionId],
        queryFn: async () => {
            const response = await collectionsApi.getVocabularies(collectionId);
            return mapVocabularies(response);
        },
        ...options,
    });
