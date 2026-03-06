import {
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Skeleton,
} from "@mui/material";

export const AllCollectionsButtonSkeleton = () => (
    <ListItemButton sx={{ px: 2, py: 1 }}>
        <ListItemIcon sx={{ minWidth: 36 }}>
            <Skeleton variant="circular" width={20} height={20} />
        </ListItemIcon>
        <ListItemText primary={<Skeleton variant="text" width="80%" />} />
    </ListItemButton>
);

export const DefaultVocabularyPinnedSkeleton = () => (
    <ListItemButton sx={{ px: 2, py: 1 }}>
        <ListItemIcon sx={{ minWidth: 36 }}>
            <Skeleton variant="circular" width={20} height={20} />
        </ListItemIcon>
        <ListItemText primary={<Skeleton variant="text" width="60%" />} />
        <Skeleton variant="text" width={16} />
    </ListItemButton>
);
