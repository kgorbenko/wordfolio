import { useState, useCallback } from "react";
import { Box, Button, IconButton, Menu, MenuItem } from "@mui/material";
import useMediaQuery from "@mui/material/useMediaQuery";
import { useTheme } from "@mui/material/styles";
import MoreVertIcon from "@mui/icons-material/MoreVert";

import styles from "./PageHeaderActions.module.scss";

export interface PageAction {
    readonly label: string;
    readonly onClick: () => void;
    readonly color?: "primary" | "error";
    readonly disabled?: boolean;
}

interface PageHeaderActionsProps {
    readonly actions: PageAction[];
}

export const PageHeaderActions = ({ actions }: PageHeaderActionsProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));
    const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);

    const handleMenuOpen = useCallback(
        (event: React.MouseEvent<HTMLElement>) => {
            setAnchorEl(event.currentTarget);
        },
        []
    );

    const handleMenuClose = useCallback(() => {
        setAnchorEl(null);
    }, []);

    if (isMobile) {
        return (
            <>
                <IconButton
                    variant="outlined"
                    onClick={handleMenuOpen}
                    className={styles.menuButton}
                >
                    <MoreVertIcon fontSize="small" />
                </IconButton>
                <Menu
                    anchorEl={anchorEl}
                    open={Boolean(anchorEl)}
                    onClose={handleMenuClose}
                >
                    {actions.map((action) => (
                        <MenuItem
                            key={action.label}
                            onClick={() => {
                                action.onClick();
                                handleMenuClose();
                            }}
                            disabled={action.disabled}
                            sx={
                                action.color === "error"
                                    ? { color: "error.main" }
                                    : undefined
                            }
                        >
                            {action.label}
                        </MenuItem>
                    ))}
                </Menu>
            </>
        );
    }

    return (
        <Box className={styles.actions}>
            {actions.map((action) => (
                <Button
                    key={action.label}
                    variant="outlined"
                    color={action.color ?? "primary"}
                    onClick={action.onClick}
                    disabled={action.disabled}
                >
                    {action.label}
                </Button>
            ))}
        </Box>
    );
};
