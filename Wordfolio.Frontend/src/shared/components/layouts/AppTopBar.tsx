import { AppBar, IconButton, Toolbar } from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";

import styles from "./AppTopBar.module.scss";

interface AppTopBarProps {
    readonly onMenuClick?: () => void;
    readonly showMenuButton?: boolean;
    readonly portalRef?: (node: HTMLDivElement | null) => void;
}

export const AppTopBar = ({
    onMenuClick,
    showMenuButton,
    portalRef,
}: AppTopBarProps) => (
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

            <div ref={portalRef} />
        </Toolbar>
    </AppBar>
);
