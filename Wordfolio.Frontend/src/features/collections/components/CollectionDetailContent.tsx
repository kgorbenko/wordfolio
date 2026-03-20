import type { GridColDef } from "@mui/x-data-grid";

import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { VocabularyWithEntryCount } from "../types";

interface CollectionDetailContentProps {
    readonly vocabularies: VocabularyWithEntryCount[];
    readonly onVocabularyClick: (id: number) => void;
    readonly onCreateVocabularyClick: () => void;
}

const desktopColumns: GridColDef<VocabularyWithEntryCount>[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.name}
                subtext={params.row.description ?? ""}
            />
        ),
    },
    {
        field: "createdAt",
        headerName: "Created At",
        type: "date",
        width: 135,
        align: "right",
        headerAlign: "right",
        valueFormatter: (value: Date) => value.toLocaleDateString(),
    },
    {
        field: "updatedAt",
        headerName: "Updated At",
        type: "date",
        width: 135,
        align: "right",
        headerAlign: "right",
        valueFormatter: (value: Date | null) => value?.toLocaleDateString(),
    },
    {
        field: "entryCount",
        headerName: "Entries",
        type: "number",
        width: 100,
        align: "right",
        headerAlign: "right",
    },
];

const mobileColumns: GridColDef<VocabularyWithEntryCount>[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.name}
                subtext={params.row.description ?? ""}
            />
        ),
    },
    {
        field: "entryCount",
        headerName: "Entries",
        type: "number",
        width: 100,
        align: "right",
        headerAlign: "right",
    },
];

export const CollectionDetailContent = ({
    vocabularies,
    onVocabularyClick,
    onCreateVocabularyClick,
}: CollectionDetailContentProps) => (
    <ContentDataGrid
        rows={vocabularies}
        desktopColumns={desktopColumns}
        mobileColumns={mobileColumns}
        onRowClick={onVocabularyClick}
        actionLabel="+ Add Vocabulary"
        onAction={onCreateVocabularyClick}
        initialSortModel={[{ field: "updatedAt", sort: "desc" }]}
    />
);
