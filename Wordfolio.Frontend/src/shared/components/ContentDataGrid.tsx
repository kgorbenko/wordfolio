import { Box } from "@mui/material";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import { DataGrid } from "@mui/x-data-grid";
import type {
    GridColDef,
    GridSortModel,
    GridValidRowModel,
} from "@mui/x-data-grid";

import { SearchActionToolbar } from "./SearchActionToolbar";
import { EmptyState } from "./EmptyState";

const SortDescIcon = () => (
    <Box
        component="span"
        className="sort-icon"
        sx={{ lineHeight: 1, color: "text.accent" }}
    >
        ↓
    </Box>
);

const SortAscIcon = () => (
    <Box
        component="span"
        className="sort-icon"
        sx={{ lineHeight: 1, color: "text.accent" }}
    >
        ↑
    </Box>
);

interface ContentDataGridProps<R extends GridValidRowModel> {
    readonly rows: R[];
    readonly desktopColumns: GridColDef<R>[];
    readonly mobileColumns: GridColDef<R>[];
    readonly onRowClick: (id: number) => void;
    readonly actionLabel: string;
    readonly onAction: () => void;
    readonly searchPlaceholder?: string;
    readonly mobileActionLabel?: string;
    readonly initialSortModel: GridSortModel;
}

export const ContentDataGrid = <R extends GridValidRowModel>({
    rows,
    desktopColumns,
    mobileColumns,
    onRowClick,
    actionLabel,
    onAction,
    searchPlaceholder = "Search...",
    mobileActionLabel = "+ New",
    initialSortModel,
}: ContentDataGridProps<R>) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    return (
        <DataGrid
            rows={rows}
            columns={isMobile ? mobileColumns : desktopColumns}
            rowHeight={isMobile ? 48 : 52}
            onRowClick={(params) => onRowClick(params.row.id)}
            showToolbar
            slots={{
                toolbar: SearchActionToolbar,
                columnSortedDescendingIcon: SortDescIcon,
                columnSortedAscendingIcon: SortAscIcon,
                noRowsOverlay: EmptyState,
            }}
            slotProps={{
                toolbar: {
                    placeholder: searchPlaceholder,
                    actionLabel,
                    mobileActionLabel,
                    onAction,
                },
            }}
            initialState={{
                sorting: {
                    sortModel: initialSortModel,
                },
            }}
            hideFooter
            sx={{ cursor: "pointer", "--DataGrid-overlayHeight": "300px" }}
        />
    );
};
