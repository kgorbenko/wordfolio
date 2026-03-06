import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { collectionsApi } from "../api/collectionsApi";
import {
    mapVocabularyWithEntryCount,
    mapToGetCollectionVocabulariesQuery,
} from "../api/mappers";
import { Vocabulary, CollectionVocabulariesQuery } from "../types";

export const useCollectionVocabulariesQuery = (
    query: CollectionVocabulariesQuery,
    options?: Partial<UseQueryOptions<Vocabulary[]>>
) =>
    useQuery({
        queryKey: ["vocabularies-summary", query.collectionId, query],
        queryFn: async () => {
            const response = await collectionsApi.getCollectionVocabularies(
                mapToGetCollectionVocabulariesQuery(query)
            );
            return response.map((v) =>
                mapVocabularyWithEntryCount(query.collectionId, v)
            );
        },
        ...options,
    });
