import { useState, MouseEvent } from "react";
import {
    AppBar,
    Toolbar,
    IconButton,
    Typography,
    Menu,
    MenuItem,
    ListItemIcon,
    ListItemText,
    Box,
    Avatar,
    useTheme,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import LogoutIcon from "@mui/icons-material/Logout";
import PersonIcon from "@mui/icons-material/Person";

import "./TopBar.scss";

interface TopBarProps {
    readonly onMenuClick?: () => void;
    readonly onLogout: () => void;
    readonly showMenuButton?: boolean;
}

export const TopBar = ({
    onMenuClick,
    onLogout,
    showMenuButton = true,
}: TopBarProps) => {
    const theme = useTheme();
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const menuOpen = Boolean(anchorEl);

    const handleUserMenuClick = (event: MouseEvent<HTMLButtonElement>) => {
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    const handleLogout = () => {
        handleMenuClose();
        onLogout();
    };

    return (
        <AppBar
            className="topbar"
            position="fixed"
            elevation={0}
            sx={{
                bgcolor: "background.paper",
                borderBottom: `1px solid ${theme.palette.divider}`,
            }}
        >
            <Toolbar>
                {showMenuButton && (
                    <IconButton
                        edge="start"
                        aria-label="open menu"
                        onClick={onMenuClick}
                        sx={{ color: "text.primary" }}
                    >
                        <MenuIcon />
                    </IconButton>
                )}

                <Box className="center-content">
                    <Typography
                        variant="h6"
                        fontWeight={700}
                        sx={{
                            background: `linear-gradient(135deg, ${theme.palette.primary.main}, ${theme.palette.secondary.main})`,
                            backgroundClip: "text",
                            WebkitBackgroundClip: "text",
                            WebkitTextFillColor: "transparent",
                        }}
                    >
                        Wordfolio
                    </Typography>
                </Box>

                <IconButton
                    edge="end"
                    aria-label="user menu"
                    onClick={handleUserMenuClick}
                    sx={{ color: "text.primary" }}
                >
                    <Avatar
                        className="user-avatar"
                        sx={{ bgcolor: "primary.main" }}
                    >
                        <PersonIcon fontSize="small" />
                    </Avatar>
                </IconButton>

                <Menu
                    anchorEl={anchorEl}
                    open={menuOpen}
                    onClose={handleMenuClose}
                    anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
                    transformOrigin={{ vertical: "top", horizontal: "right" }}
                >
                    <MenuItem onClick={handleLogout}>
                        <ListItemIcon>
                            <LogoutIcon fontSize="small" />
                        </ListItemIcon>
                        <ListItemText>Logout</ListItemText>
                    </MenuItem>
                </Menu>
            </Toolbar>
        </AppBar>
    );
};
