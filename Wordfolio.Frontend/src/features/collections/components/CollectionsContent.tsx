import FolderIcon from "@mui/icons-material/Folder";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { GridColDef } from "@mui/x-data-grid";
import { DataGridWithFilter } from "../../../shared/components/DataGridWithFilter";
import { EmptyState } from "../../../shared/components/EmptyState";
import { CollectionWithVocabularyCount } from "../types";

interface CollectionsContentProps {
    readonly collections: CollectionWithVocabularyCount[];
    readonly onCollectionClick: (id: number) => void;
    readonly onCreateClick: () => void;
}

const columns: GridColDef<CollectionWithVocabularyCount>[] = [
    { field: "name", headerName: "Name", flex: 2 },
    {
        field: "description",
        headerName: "Description",
        flex: 3,
        renderCell: (params) => params.value,
    },
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
        field: "vocabularyCount",
        headerName: "Vocabularies",
        type: "number",
        width: 140,
    },
];

const mobileColumnVisibility = {
    description: false,
    createdAt: false,
    updatedAt: false,
};

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
                actionLabel="Create Collection"
                onAction={onCreateClick}
            />
        );
    }

    return (
        <DataGridWithFilter
            rows={collections}
            columns={columns}
            columnVisibilityModel={
                isMobile ? mobileColumnVisibility : undefined
            }
            onRowClick={(params) => onCollectionClick(params.row.id)}
            initialState={{
                sorting: {
                    sortModel: [{ field: "updatedAt", sort: "desc" }],
                },
            }}
        />
    );
};
