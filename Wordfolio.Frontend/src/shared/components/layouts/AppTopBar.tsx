import { AppBar, Box, IconButton, Toolbar } from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";

import styles from "./AppTopBar.module.scss";
import { WordfolioBrand } from "./WordfolioBrand";

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
    <AppBar position="fixed" className={styles.appTopBar}>
        <Toolbar className={styles.appToolbar}>
            {!showMenuButton && (
                <Box className={styles.brandZone} sx={{ width: 278 }}>
                    <WordfolioBrand />
                </Box>
            )}

            <div ref={portalRef} className={styles.portalSlot} />

            {showMenuButton && (
                <IconButton
                    edge="end"
                    aria-label="open menu"
                    onClick={onMenuClick}
                    className={styles.menuButton}
                    sx={{ color: "text.topbarPrimary" }}
                >
                    <MenuIcon />
                </IconButton>
            )}
        </Toolbar>
    </AppBar>
);
