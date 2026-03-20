import { Box } from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { DataGrid } from "@mui/x-data-grid";
import type { GridColDef } from "@mui/x-data-grid";

import { SearchActionToolbar } from "../../../shared/components/SearchActionToolbar";
import { TextWithSubtext } from "../../../shared/components/TextWithSubtext";
import { EmptyState } from "../../../shared/components/EmptyState";
import { CollectionWithVocabularyCount } from "../types";

interface CollectionsContentProps {
    readonly collections: CollectionWithVocabularyCount[];
    readonly onCollectionClick: (id: number) => void;
    readonly onCreateClick: () => void;
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

const desktopColumns: GridColDef<CollectionWithVocabularyCount>[] = [
    {
        field: "name",
        headerName: "Collection",
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
        field: "vocabularyCount",
        headerName: "Vocabs",
        type: "number",
        width: 90,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
];

const mobileColumns: GridColDef<CollectionWithVocabularyCount>[] = [
    {
        field: "name",
        headerName: "Collection",
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
        field: "vocabularyCount",
        headerName: "Vocabs",
        type: "number",
        width: 75,
        align: "right",
        headerAlign: "right",
        sortable: false,
    },
];

export const CollectionsContent = ({
    collections,
    onCollectionClick,
    onCreateClick,
}: CollectionsContentProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    if (collections.length === 0) {
        return (
            <EmptyState
                icon={<FolderIcon />}
                title="No Collections Yet"
                description="Create your first collection to organize words."
            />
        );
    }

    return (
        <DataGrid
            rows={collections}
            columns={isMobile ? mobileColumns : desktopColumns}
            rowHeight={isMobile ? 48 : 52}
            onRowClick={(params) => onCollectionClick(params.row.id)}
            showToolbar
            slots={{
                toolbar: SearchActionToolbar,
                columnSortedDescendingIcon: SortDescIcon,
                columnSortedAscendingIcon: SortAscIcon,
            }}
            slotProps={{
                toolbar: {
                    placeholder: "Search collections...",
                    actionLabel: "+ Add Collection",
                    mobileActionLabel: "+ New",
                    onAction: onCreateClick,
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
