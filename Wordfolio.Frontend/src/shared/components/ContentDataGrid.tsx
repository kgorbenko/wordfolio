import { ReactNode, useMemo } from "react";
import { Box } from "@mui/material";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import SearchOffIcon from "@mui/icons-material/SearchOff";
import { DataGrid } from "@mui/x-data-grid";
import type {
    GridColDef,
    GridFilterModel,
    GridSortModel,
    GridValidRowModel,
} from "@mui/x-data-grid";

import { SearchActionToolbar } from "./SearchActionToolbar";
import { EmptyState } from "./EmptyState";

declare module "@mui/x-data-grid" {
    interface NoResultsOverlayPropsOverrides {
        icon?: ReactNode;
        title?: string;
        description?: string;
    }
}

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
    readonly sortModel: GridSortModel;
    readonly onSortModelChange: (model: GridSortModel) => void;
    readonly filterValue: string;
    readonly onFilterValueChange: (value: string) => void;
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
    sortModel,
    onSortModelChange,
    filterValue,
    onFilterValueChange,
}: ContentDataGridProps<R>) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const activeColumns = isMobile ? mobileColumns : desktopColumns;

    const validSortModel = useMemo<GridSortModel>(
        () =>
            sortModel.filter((item) =>
                activeColumns.some((col) => col.field === item.field)
            ),
        [sortModel, activeColumns]
    );

    const filterModel = useMemo<GridFilterModel>(
        () => ({
            items: [],
            quickFilterValues: filterValue
                ? filterValue.trim().split(/\s+/)
                : [],
        }),
        [filterValue]
    );

    return (
        <DataGrid
            rows={rows}
            columns={activeColumns}
            rowHeight={isMobile ? 47 : 51}
            onRowClick={(params) => onRowClick(params.row.id)}
            showToolbar
            slots={{
                toolbar: SearchActionToolbar,
                columnSortedDescendingIcon: SortDescIcon,
                columnSortedAscendingIcon: SortAscIcon,
                noRowsOverlay: EmptyState,
                noResultsOverlay: EmptyState,
            }}
            slotProps={{
                toolbar: {
                    placeholder: searchPlaceholder,
                    actionLabel,
                    mobileActionLabel,
                    onAction,
                },
                noResultsOverlay: {
                    icon: (
                        <SearchOffIcon
                            sx={{ fontSize: 32, color: "secondary.main" }}
                        />
                    ),
                    title: "No matches found",
                    description: "Try adjusting your search",
                },
            }}
            sortModel={validSortModel}
            onSortModelChange={onSortModelChange}
            sortingOrder={["asc", "desc"]}
            filterModel={filterModel}
            onFilterModelChange={(m) => {
                const newValue = m.quickFilterValues?.join(" ") ?? "";
                if (newValue !== filterValue) {
                    onFilterValueChange(newValue);
                }
            }}
            hideFooter
            sx={{ cursor: "pointer", "--DataGrid-overlayHeight": "300px" }}
        />
    );
};
