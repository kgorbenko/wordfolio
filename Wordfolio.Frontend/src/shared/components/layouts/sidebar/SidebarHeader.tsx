import { Box, Button, Typography, useTheme } from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";
import AddIcon from "@mui/icons-material/Add";
import styles from "./SidebarHeader.module.scss";

interface SidebarHeaderProps {
    readonly onHomeClick: () => void;
    readonly onAddEntry?: () => void;
}

export const SidebarHeader = ({
    onHomeClick,
    onAddEntry,
}: SidebarHeaderProps) => {
    const theme = useTheme();

    return (
        <>
            <Box
                className={styles.logoContainer}
                sx={{ borderBottom: `1px solid ${theme.palette.divider}` }}
                onClick={onHomeClick}
            >
                <Box
                    className={styles.logoIconWrapper}
                    sx={{ bgcolor: "primary.main" }}
                >
                    <MenuBookIcon sx={{ color: "white", fontSize: 18 }} />
                </Box>
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

            <Box className={styles.addWordContainer}>
                <Button
                    variant="contained"
                    fullWidth
                    startIcon={<AddIcon />}
                    onClick={onAddEntry}
                    className={styles.addWordButton}
                >
                    Add Entry
                </Button>
            </Box>
        </>
    );
};
