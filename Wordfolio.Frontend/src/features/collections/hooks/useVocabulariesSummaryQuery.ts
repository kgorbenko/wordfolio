import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import {
    collectionsApi,
    GetVocabulariesSummaryQuery,
} from "../api/collectionsApi";
import { mapVocabularySummary } from "../api/mappers";
import { Vocabulary } from "../types";

export const useVocabulariesSummaryQuery = (
    query: GetVocabulariesSummaryQuery,
    options?: Partial<UseQueryOptions<Vocabulary[]>>
) =>
    useQuery({
        queryKey: ["vocabularies-summary", query.collectionId, query],
        queryFn: async () => {
            const response = await collectionsApi.getVocabulariesSummary(query);
            return response.map((v) =>
                mapVocabularySummary(query.collectionId, v)
            );
        },
        ...options,
    });
