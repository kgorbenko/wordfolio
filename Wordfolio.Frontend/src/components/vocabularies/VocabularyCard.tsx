import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
    Chip,
} from "@mui/material";
import MenuBookIcon from "@mui/icons-material/MenuBook";

import "./VocabularyCard.scss";

interface VocabularyCardProps {
    readonly id: number;
    readonly name: string;
    readonly description?: string;
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
        <Card className="vocabulary-card" sx={{ "&:hover": { boxShadow: 4 } }}>
            <CardActionArea className="action-area" onClick={onClick}>
                <CardContent className="content">
                    <Box className="header">
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
                            className="description"
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
