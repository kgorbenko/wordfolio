import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import { Collection } from "../types";
import styles from "./CollectionCard.module.scss";

interface CollectionCardProps {
    readonly collection: Collection;
    readonly onClick: () => void;
}

export const CollectionCard = ({
    collection,
    onClick,
}: CollectionCardProps) => (
    <Card className={styles.card}>
        <CardActionArea className={styles.actionArea} onClick={onClick}>
            <CardContent className={styles.content}>
                <Box className={styles.header}>
                    <FolderIcon sx={{ color: "primary.main", fontSize: 24 }} />
                    <Typography variant="h6" className={styles.title} noWrap>
                        {collection.name}
                    </Typography>
                </Box>
                <Typography
                    variant="body2"
                    color="text.secondary"
                    className={styles.description}
                >
                    {collection.description || "No description"}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                    {collection.vocabularyCount}{" "}
                    {collection.vocabularyCount === 1
                        ? "vocabulary"
                        : "vocabularies"}
                </Typography>
            </CardContent>
        </CardActionArea>
    </Card>
);
