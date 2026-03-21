import type { GridColDef } from "@mui/x-data-grid";

import { ContentDataGrid } from "../../../shared/components/ContentDataGrid";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { Entry } from "../../../shared/types/entries";

interface DraftsContentProps {
    readonly entries: Entry[];
    readonly onEntryClick: (id: number) => void;
    readonly onAddDraftClick: () => void;
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
        headerName: "Created At",
        type: "date",
        width: 120,
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
        field: "translationCount",
        headerName: "Translations",
        type: "number",
        width: 115,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Definitions",
        type: "number",
        width: 110,
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
        minWidth: 150,
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
        width: 75,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Defs.",
        type: "number",
        width: 70,
        align: "right",
        headerAlign: "right",
        valueGetter: (_, row) => row.definitions.length,
    },
];

export const DraftsContent = ({
    entries,
    onEntryClick,
    onAddDraftClick,
}: DraftsContentProps) => (
    <ContentDataGrid
        rows={entries}
        desktopColumns={desktopColumns}
        mobileColumns={mobileColumns}
        onRowClick={onEntryClick}
        actionLabel="+ Add Draft"
        onAction={onAddDraftClick}
        initialSortModel={[{ field: "updatedAt", sort: "desc" }]}
    />
);
