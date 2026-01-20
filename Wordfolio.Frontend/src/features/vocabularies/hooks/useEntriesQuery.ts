import { useQuery, UseQueryOptions } from "@tanstack/react-query";
import { entriesApi, EntryResponse } from "../../../api/entriesApi";

export const useEntriesQuery = (
    vocabularyId: number,
    options?: Partial<UseQueryOptions<EntryResponse[]>>
) =>
    useQuery({
        queryKey: ["entries", vocabularyId],
        queryFn: () => entriesApi.getEntries(vocabularyId),
        ...options,
    });
