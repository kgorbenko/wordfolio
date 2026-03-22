import type { GridColDef } from "@mui/x-data-grid";

import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { CollectionWithVocabularyCount } from "../types";

interface CollectionsContentProps {
    readonly collections: CollectionWithVocabularyCount[];
    readonly onCollectionClick: (id: number) => void;
    readonly onCreateClick: () => void;
}

const desktopColumns: GridColDef<CollectionWithVocabularyCount>[] = [
    {
        field: "name",
        headerName: "Collection",
        flex: 1,
        minWidth: 200,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.name}
                subtext={params.row.description ?? ""}
            />
        ),
    },
    {
        field: "createdAt",
        headerName: "Created",
        type: "date",
        width: 115,
        align: "right",
        headerAlign: "right",
        valueFormatter: (value: Date) => value.toLocaleDateString(),
    },
    {
        field: "updatedAt",
        headerName: "Updated",
        type: "date",
        width: 115,
        align: "right",
        headerAlign: "right",
        valueFormatter: (value: Date | null) => value?.toLocaleDateString(),
    },
    {
        field: "vocabularyCount",
        headerName: "Vocabs",
        type: "number",
        width: 105,
        align: "right",
        headerAlign: "right",
    },
];

const mobileColumns: GridColDef<CollectionWithVocabularyCount>[] = [
    {
        field: "name",
        headerName: "Collection",
        flex: 1,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.name}
                subtext={params.row.description ?? ""}
            />
        ),
    },
    {
        field: "vocabularyCount",
        headerName: "Vocabs",
        type: "number",
        width: 95,
        align: "right",
        headerAlign: "right",
    },
];

export const CollectionsContent = ({
    collections,
    onCollectionClick,
    onCreateClick,
}: CollectionsContentProps) => (
    <ContentDataGrid
        rows={collections}
        desktopColumns={desktopColumns}
        mobileColumns={mobileColumns}
        onRowClick={onCollectionClick}
        actionLabel="+ Add Collection"
        onAction={onCreateClick}
        initialSortModel={[{ field: "updatedAt", sort: "desc" }]}
    />
);
