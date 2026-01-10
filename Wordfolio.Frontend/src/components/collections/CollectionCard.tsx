import { useState, MouseEvent } from "react";
import {
    Card,
    CardActionArea,
    CardContent,
    Typography,
    Box,
    IconButton,
    Menu,
    MenuItem,
    ListItemIcon,
    ListItemText,
} from "@mui/material";
import FolderIcon from "@mui/icons-material/Folder";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";

import "./CollectionCard.scss";

interface CollectionCardProps {
    readonly id: number;
    readonly name: string;
    readonly description?: string;
    readonly vocabularyCount: number;
    readonly onClick?: () => void;
    readonly onEdit?: () => void;
    readonly onDelete?: () => void;
}

export const CollectionCard = ({
    name,
    description,
    vocabularyCount,
    onClick,
    onEdit,
    onDelete,
}: CollectionCardProps) => {
    const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
    const menuOpen = Boolean(anchorEl);

    const handleMenuClick = (event: MouseEvent<HTMLButtonElement>) => {
        event.stopPropagation();
        setAnchorEl(event.currentTarget);
    };

    const handleMenuClose = () => {
        setAnchorEl(null);
    };

    const handleEdit = () => {
        handleMenuClose();
        onEdit?.();
    };

    const handleDelete = () => {
        handleMenuClose();
        onDelete?.();
    };

    return (
        <Card className="collection-card" sx={{ "&:hover": { boxShadow: 4 } }}>
            <CardActionArea className="action-area" onClick={onClick}>
                <CardContent className="content" sx={{ pr: 5 }}>
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

            <IconButton
                className="menu-button"
                size="small"
                onClick={handleMenuClick}
                sx={{
                    bgcolor: "background.paper",
                    "&:hover": { bgcolor: "action.hover" },
                }}
            >
                <MoreVertIcon fontSize="small" />
            </IconButton>

            <Menu
                anchorEl={anchorEl}
                open={menuOpen}
                onClose={handleMenuClose}
                anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
                transformOrigin={{ vertical: "top", horizontal: "right" }}
            >
                <MenuItem onClick={handleEdit}>
                    <ListItemIcon>
                        <EditIcon fontSize="small" />
                    </ListItemIcon>
                    <ListItemText>Edit</ListItemText>
                </MenuItem>
                <MenuItem onClick={handleDelete} sx={{ color: "error.main" }}>
                    <ListItemIcon>
                        <DeleteIcon fontSize="small" color="error" />
                    </ListItemIcon>
                    <ListItemText>Delete</ListItemText>
                </MenuItem>
            </Menu>
        </Card>
    );
};
