import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import { mapVocabularyWithEntryCount } from "../api/mappers";
import { Vocabulary } from "../types";

export const useCollectionVocabulariesQuery = (
    collectionId: number,
    options?: Partial<UseQueryOptions<Vocabulary[]>>
) =>
    useQuery({
        queryKey: ["vocabularies-summary", collectionId],
        queryFn: async () => {
            const response =
                await collectionsApi.getCollectionVocabularies(collectionId);
            return response.map((v) =>
                mapVocabularyWithEntryCount(collectionId, v)
            );
        },
        ...options,
    });
