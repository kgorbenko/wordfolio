import { Box } from "@mui/material";
import styles from "./CardGrid.module.scss";

interface CardGridProps {
    readonly children: React.ReactNode;
}

export const CardGrid = ({ children }: CardGridProps) => (
    <Box className={styles.grid}>{children}</Box>
);
