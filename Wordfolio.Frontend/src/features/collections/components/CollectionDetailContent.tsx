import MenuBookIcon from "@mui/icons-material/MenuBook";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { GridColDef } from "@mui/x-data-grid";
import { DataGridWithFilter } from "../../../shared/components/DataGridWithFilter";
import { EmptyState } from "../../../shared/components/EmptyState";
import { VocabularyWithEntryCount } from "../types";

interface CollectionDetailContentProps {
    readonly vocabularies: VocabularyWithEntryCount[];
    readonly onVocabularyClick: (id: number) => void;
    readonly onCreateVocabularyClick: () => void;
}

const columns: GridColDef<VocabularyWithEntryCount>[] = [
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
        field: "entryCount",
        headerName: "Entries",
        type: "number",
        width: 100,
    },
];

const mobileColumnVisibility = {
    description: false,
    createdAt: false,
    updatedAt: false,
};

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
                actionLabel="Add Vocabulary"
                onAction={onCreateVocabularyClick}
            />
        );
    }

    return (
        <DataGridWithFilter
            rows={vocabularies}
            columns={columns}
            columnVisibilityModel={
                isMobile ? mobileColumnVisibility : undefined
            }
            onRowClick={(params) => onVocabularyClick(params.row.id)}
            initialState={{
                sorting: {
                    sortModel: [{ field: "updatedAt", sort: "desc" }],
                },
            }}
        />
    );
};
