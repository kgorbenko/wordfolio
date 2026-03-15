import MenuBookIcon from "@mui/icons-material/MenuBook";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { GridColDef } from "@mui/x-data-grid";

import { DataGridWithFilter } from "../../../shared/components/DataGridWithFilter";
import { EmptyState } from "../../../shared/components/EmptyState";
import { Entry } from "../../../shared/types/entries";

interface VocabularyDetailContentProps {
    readonly entries: Entry[];
    readonly onEntryClick: (id: number) => void;
    readonly onAddWordClick: () => void;
}

const columns: GridColDef<Entry>[] = [
    { field: "entryText", headerName: "Entry", flex: 2 },
    {
        field: "createdAt",
        headerName: "Created At",
        type: "date",
        width: 120,
        valueFormatter: (value: Date) => value.toLocaleDateString(),
    },
    {
        field: "updatedAt",
        headerName: "Updated At",
        type: "date",
        width: 120,
        valueFormatter: (value: Date | null) => value?.toLocaleDateString(),
    },
    {
        field: "translationCount",
        headerName: "Translations",
        type: "number",
        width: 130,
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Definitions",
        type: "number",
        width: 120,
        valueGetter: (_, row) => row.definitions.length,
    },
];

const mobileColumnVisibility = {
    createdAt: false,
    updatedAt: false,
};

export const VocabularyDetailContent = ({
    entries,
    onEntryClick,
    onAddWordClick,
}: VocabularyDetailContentProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    if (entries.length === 0) {
        return (
            <EmptyState
                icon={
                    <MenuBookIcon
                        sx={{ fontSize: 40, color: "secondary.main" }}
                    />
                }
                title="No Words Yet"
                description="Tap the + button to add your first word to this vocabulary."
                actionLabel="Add Word"
                onAction={onAddWordClick}
            />
        );
    }

    return (
        <DataGridWithFilter
            rows={entries}
            columns={columns}
            columnVisibilityModel={
                isMobile ? mobileColumnVisibility : undefined
            }
            onRowClick={(params) => onEntryClick(params.row.id)}
            initialState={{
                sorting: {
                    sortModel: [{ field: "updatedAt", sort: "desc" }],
                },
            }}
        />
    );
};
