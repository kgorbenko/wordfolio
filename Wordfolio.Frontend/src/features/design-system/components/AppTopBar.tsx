import { AppBar, IconButton, TextField, Toolbar } from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import SearchIcon from "@mui/icons-material/Search";

import { BreadcrumbNav } from "./BreadcrumbNav";
import type { BreadcrumbItem } from "./BreadcrumbNav";
import styles from "./AppTopBar.module.scss";

export type { BreadcrumbItem };

interface AppTopBarProps {
    readonly onMenuClick?: () => void;
    readonly showMenuButton?: boolean;
    readonly breadcrumbs: BreadcrumbItem[];
}

export const AppTopBar = ({ onMenuClick, showMenuButton, breadcrumbs }: AppTopBarProps) => (
    <AppBar position="static" className={styles.appTopBar}>
        <Toolbar className={styles.appToolbar}>
            {showMenuButton && (
                <IconButton
                    edge="start"
                    aria-label="open menu"
                    onClick={onMenuClick}
                    className={styles.menuButton}
                    sx={{ color: "text.primary" }}
                >
                    <MenuIcon />
                </IconButton>
            )}

            <BreadcrumbNav items={breadcrumbs} truncate={showMenuButton} />

            {!showMenuButton && (
                <div className={styles.searchBox}>
                    <TextField
                        placeholder="Search..."
                        className={styles.search}
                        slotProps={{
                            input: {
                                startAdornment: (
                                    <SearchIcon
                                        sx={{ fontSize: 12, color: "text.disabled", mr: 1 }}
                                    />
                                ),
                            },
                        }}
                    />
                </div>
            )}
        </Toolbar>
    </AppBar>
);
