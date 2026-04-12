import type { GridColDef, GridSortModel } from "@mui/x-data-grid";

import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { Entry } from "../../../shared/api/types/entries";

interface DraftsContentProps {
    readonly entries: Entry[];
    readonly onEntryClick: (id: string) => void;
    readonly onAddDraftClick: () => void;
    readonly sortModel: GridSortModel;
    readonly onSortModelChange: (model: GridSortModel) => void;
    readonly filterValue: string;
    readonly onFilterValueChange: (value: string) => void;
}

const desktopColumns: GridColDef<Entry>[] = [
    {
        field: "entryText",
        headerName: "Entry",
        flex: 1,
        minWidth: 200,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.entryText}
                subtext={
                    params.row.definitions[0]?.definitionText ??
                    params.row.translations[0]?.translationText ??
                    ""
                }
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
        field: "translationCount",
        headerName: "Trans.",
        type: "number",
        width: 100,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Defs.",
        type: "number",
        width: 95,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.definitions.length,
    },
];

const mobileColumns: GridColDef<Entry>[] = [
    {
        field: "entryText",
        headerName: "Entry",
        flex: 1,
        renderCell: (params) => (
            <TextWithSubtext
                text={params.row.entryText}
                subtext={
                    params.row.definitions[0]?.definitionText ??
                    params.row.translations[0]?.translationText ??
                    ""
                }
            />
        ),
    },
    {
        field: "translationCount",
        headerName: "Trans.",
        type: "number",
        width: 90,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Defs.",
        type: "number",
        width: 80,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.definitions.length,
    },
];

export const DraftsContent = ({
    entries,
    onEntryClick,
    onAddDraftClick,
    sortModel,
    onSortModelChange,
    filterValue,
    onFilterValueChange,
}: DraftsContentProps) => (
    <ContentDataGrid
        rows={entries}
        desktopColumns={desktopColumns}
        mobileColumns={mobileColumns}
        onRowClick={onEntryClick}
        actionLabel="+ Add Draft"
        onAction={onAddDraftClick}
        sortModel={sortModel}
        onSortModelChange={onSortModelChange}
        filterValue={filterValue}
        onFilterValueChange={onFilterValueChange}
    />
);
