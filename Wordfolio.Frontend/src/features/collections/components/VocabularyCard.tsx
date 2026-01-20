import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
    Chip,
} from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";

import styles from "./VocabularyCard.module.scss";

interface VocabularyCardProps {
    readonly id: number;
    readonly name: string;
    readonly description?: string | null;
    readonly entryCount: number;
    readonly onClick?: () => void;
}

export const VocabularyCard = ({
    name,
    description,
    entryCount,
    onClick,
}: VocabularyCardProps) => {
    return (
        <Card className={styles.container}>
            <CardActionArea className={styles.actionArea} onClick={onClick}>
                <CardContent className={styles.content}>
                    <Box className={styles.header}>
                        <MenuBookIcon
                            sx={{ color: "secondary.main", fontSize: 24 }}
                        />
                        <Typography
                            variant="h6"
                            fontWeight={600}
                            noWrap
                            sx={{ flex: 1 }}
                        >
                            {name}
                        </Typography>
                        <Chip
                            label={entryCount}
                            size="small"
                            sx={{
                                bgcolor: "primary.main",
                                color: "white",
                                fontWeight: 600,
                                minWidth: 32,
                            }}
                        />
                    </Box>
                    {description && (
                        <Typography
                            className={styles.description}
                            variant="body2"
                            color="text.secondary"
                        >
                            {description}
                        </Typography>
                    )}
                </CardContent>
            </CardActionArea>
        </Card>
    );
};
