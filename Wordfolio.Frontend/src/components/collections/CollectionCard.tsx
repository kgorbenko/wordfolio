import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";

import "./CollectionCard.scss";

interface CollectionCardProps {
    readonly id: number;
    readonly name: string;
    readonly description?: string;
    readonly vocabularyCount: number;
    readonly onClick?: () => void;
}

export const CollectionCard = ({
    name,
    description,
    vocabularyCount,
    onClick,
}: CollectionCardProps) => {
    return (
        <Card className="collection-card" sx={{ "&:hover": { boxShadow: 4 } }}>
            <CardActionArea className="action-area" onClick={onClick}>
                <CardContent className="content">
                    <Box className="header">
                        <FolderIcon
                            sx={{ color: "primary.main", fontSize: 24 }}
                        />
                        <Typography variant="h6" fontWeight={600} noWrap>
                            {name}
                        </Typography>
                    </Box>
                    <Typography
                        className="description"
                        variant="body2"
                        color="text.secondary"
                    >
                        {description || "No description"}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                        {vocabularyCount}{" "}
                        {vocabularyCount === 1 ? "vocabulary" : "vocabularies"}
                    </Typography>
                </CardContent>
            </CardActionArea>
        </Card>
    );
};
