import { Box, Button, InputAdornment, TextField } from "@mui/material";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import SearchIcon from "@mui/icons-material/Search";
import ClearIcon from "@mui/icons-material/Clear";
import {
    QuickFilter,
    QuickFilterControl,
    QuickFilterClear,
} from "@mui/x-data-grid";

import styles from "./SearchActionToolbar.module.scss";

declare module "@mui/x-data-grid" {
    interface ToolbarPropsOverrides {
        placeholder?: string;
        actionLabel?: string;
        mobileActionLabel?: string;
        onAction?: () => void;
    }
}

export interface SearchActionToolbarProps {
    readonly placeholder?: string;
    readonly actionLabel?: string;
    readonly mobileActionLabel?: string;
    readonly onAction?: () => void;
}

export const SearchActionToolbar = ({
    placeholder = "Search...",
    actionLabel = "Add",
    mobileActionLabel,
    onAction,
}: SearchActionToolbarProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    return (
        <Box className={styles.toolbar}>
            <QuickFilter className={styles.filter}>
                <QuickFilterControl
                    render={({ ref, ...controlProps }, state) => (
                        <TextField
                            {...controlProps}
                            inputRef={ref}
                            placeholder={placeholder}
                            fullWidth
                            slotProps={{
                                input: {
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <SearchIcon />
                                        </InputAdornment>
                                    ),
                                    endAdornment: state.value ? (
                                        <InputAdornment position="end">
                                            <QuickFilterClear
                                                size="small"
                                                edge="end"
                                                aria-label="Clear search"
                                            >
                                                <ClearIcon />
                                            </QuickFilterClear>
                                        </InputAdornment>
                                    ) : null,
                                    ...controlProps.slotProps?.input,
                                },
                                ...controlProps.slotProps,
                            }}
                        />
                    )}
                />
            </QuickFilter>
            <Button
                variant="contained"
                onClick={onAction}
                className={styles.actionButton}
            >
                {isMobile ? (mobileActionLabel ?? actionLabel) : actionLabel}
            </Button>
        </Box>
    );
};
