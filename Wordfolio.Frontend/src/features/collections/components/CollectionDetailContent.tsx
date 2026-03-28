import type { GridColDef, GridSortModel } from "@mui/x-data-grid";

import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import type { VocabularyWithEntryCount } from "../../../shared/api/types/collections";

interface CollectionDetailContentProps {
    readonly vocabularies: VocabularyWithEntryCount[];
    readonly onVocabularyClick: (id: number) => void;
    readonly onCreateVocabularyClick: () => void;
    readonly sortModel: GridSortModel;
    readonly onSortModelChange: (model: GridSortModel) => void;
    readonly filterValue: string;
    readonly onFilterValueChange: (value: string) => void;
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
        width: 95,
        align: "right",
        headerAlign: "right",
    },
];

export const CollectionDetailContent = ({
    vocabularies,
    onVocabularyClick,
    onCreateVocabularyClick,
    sortModel,
    onSortModelChange,
    filterValue,
    onFilterValueChange,
}: CollectionDetailContentProps) => (
    <ContentDataGrid
        rows={vocabularies}
        desktopColumns={desktopColumns}
        mobileColumns={mobileColumns}
        onRowClick={onVocabularyClick}
        actionLabel="+ Add Vocabulary"
        onAction={onCreateVocabularyClick}
        sortModel={sortModel}
        onSortModelChange={onSortModelChange}
        filterValue={filterValue}
        onFilterValueChange={onFilterValueChange}
    />
);
