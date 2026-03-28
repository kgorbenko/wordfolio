import { useQuery, type UseQueryOptions } from "@tanstack/react-query";

import { client } from "../client";
import {
    mapCollectionDetail,
    mapCollectionWithVocabularyCount,
    mapCollectionsHierarchy,
    mapVocabularyWithEntryCount,
} from "../mappers/collections";
import type {
    Collection,
    CollectionWithVocabularyCount,
    CollectionsHierarchy,
    VocabularyWithEntryCount,
} from "../types/collections";

export const useCollectionsQuery = (
    options?: Partial<UseQueryOptions<CollectionWithVocabularyCount[]>>
) =>
    useQuery({
        queryKey: ["collections"],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/collections-hierarchy/collections"
            );
            if (error) throw error;
            return data!.map(mapCollectionWithVocabularyCount);
        },
        ...options,
    });

export const useCollectionQuery = (
    id: number,
    options?: Partial<UseQueryOptions<Collection>>
) =>
    useQuery({
        queryKey: ["collections", id],
        queryFn: async () => {
            const { data, error } = await client.GET("/collections/{id}", {
                params: { path: { id } },
            });
            if (error) throw error;
            return mapCollectionDetail(data!);
        },
        ...options,
    });

export const useCollectionsHierarchyQuery = (
    options?: Partial<UseQueryOptions<CollectionsHierarchy>>
) =>
    useQuery({
        queryKey: ["collections-hierarchy"],
        queryFn: async () => {
            const { data, error } = await client.GET("/collections-hierarchy");
            if (error) throw error;
            return mapCollectionsHierarchy(data!);
        },
        ...options,
    });

export const useCollectionVocabulariesQuery = (
    collectionId: number,
    options?: Partial<UseQueryOptions<VocabularyWithEntryCount[]>>
) =>
    useQuery({
        queryKey: ["vocabularies-summary", collectionId],
        queryFn: async () => {
            const { data, error } = await client.GET(
                "/collections-hierarchy/collections/{collectionId}/vocabularies",
                { params: { path: { collectionId } } }
            );
            if (error) throw error;
            return data!.map((v) =>
                mapVocabularyWithEntryCount(v, collectionId)
            );
        },
        ...options,
    });
