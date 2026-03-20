import { Box } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";

import { SearchActionToolbar } from "../../../shared/components/SearchActionToolbar";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { EmptyState } from "../../../shared/components/EmptyState";
import { Entry } from "../../../shared/types/entries";

interface VocabularyDetailContentProps {
    readonly entries: Entry[];
    readonly onEntryClick: (id: number) => void;
    readonly onAddWordClick: () => void;
}

const SortDescIcon = () => (
    <Box
        component="span"
        sx={{ fontSize: 11, lineHeight: 1, color: "text.accent" }}
    >
        ↓
    </Box>
);

const SortAscIcon = () => (
    <Box
        component="span"
        sx={{ fontSize: 11, lineHeight: 1, color: "text.accent" }}
    >
        ↑
    </Box>
);

const desktopColumns: GridColDef<Entry>[] = [
    {
        field: "entryText",
        headerName: "Entry",
        flex: 1,
        minWidth: 200,
        sortable: false,
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
        sortable: false,
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
        sortable: false,
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Definitions",
        type: "number",
        width: 110,
        align: "right",
        headerAlign: "right",
        sortable: false,
        valueGetter: (_, row) => row.definitions.length,
    },
];

const mobileColumns: GridColDef<Entry>[] = [
    {
        field: "entryText",
        headerName: "Entry",
        flex: 1,
        minWidth: 150,
        sortable: false,
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
        sortable: false,
        valueGetter: (_, row) => row.translations.length,
    },
    {
        field: "definitionCount",
        headerName: "Defs.",
        type: "number",
        width: 70,
        align: "right",
        headerAlign: "right",
        sortable: false,
        valueGetter: (_, row) => row.definitions.length,
    },
];

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
            />
        );
    }

    return (
        <DataGrid
            rows={entries}
            columns={isMobile ? mobileColumns : desktopColumns}
            rowHeight={isMobile ? 48 : 52}
            onRowClick={(params) => onEntryClick(params.row.id)}
            showToolbar
            slots={{
                toolbar: SearchActionToolbar,
                columnSortedDescendingIcon: SortDescIcon,
                columnSortedAscendingIcon: SortAscIcon,
            }}
            slotProps={{
                toolbar: {
                    placeholder: "Search entries...",
                    actionLabel: "+ Add Entry",
                    mobileActionLabel: "+ New",
                    onAction: onAddWordClick,
                },
            }}
            initialState={{
                sorting: {
                    sortModel: [{ field: "updatedAt", sort: "desc" }],
                },
            }}
            hideFooter
            sx={{ cursor: "pointer" }}
        />
    );
};
