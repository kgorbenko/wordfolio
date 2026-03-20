import { Box } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";

import { SearchActionToolbar } from "../../../shared/components/SearchActionToolbar";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { EmptyState } from "../../../shared/components/EmptyState";
import { VocabularyWithEntryCount } from "../types";

interface CollectionDetailContentProps {
    readonly vocabularies: VocabularyWithEntryCount[];
    readonly onVocabularyClick: (id: number) => void;
    readonly onCreateVocabularyClick: () => void;
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

const desktopColumns: GridColDef<VocabularyWithEntryCount>[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        minWidth: 200,
        sortable: false,
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
        field: "entryCount",
        headerName: "Entries",
        type: "number",
        width: 90,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
];

const mobileColumns: GridColDef<VocabularyWithEntryCount>[] = [
    {
        field: "name",
        headerName: "Vocabulary",
        flex: 1,
        minWidth: 150,
        sortable: false,
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
        width: 80,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
];

export const CollectionDetailContent = ({
    vocabularies,
    onVocabularyClick,
    onCreateVocabularyClick,
}: CollectionDetailContentProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    if (vocabularies.length === 0) {
        return (
            <EmptyState
                icon={<MenuBookIcon />}
                title="No Vocabularies Yet"
                description="Add your first vocabulary - a book, movie, or any source of new words."
            />
        );
    }

    return (
        <DataGrid
            rows={vocabularies}
            columns={isMobile ? mobileColumns : desktopColumns}
            rowHeight={isMobile ? 48 : 52}
            onRowClick={(params) => onVocabularyClick(params.row.id)}
            showToolbar
            slots={{
                toolbar: SearchActionToolbar,
                columnSortedDescendingIcon: SortDescIcon,
                columnSortedAscendingIcon: SortAscIcon,
            }}
            slotProps={{
                toolbar: {
                    placeholder: "Search vocabularies...",
                    actionLabel: "+ Add Vocabulary",
                    mobileActionLabel: "+ New",
                    onAction: onCreateVocabularyClick,
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
