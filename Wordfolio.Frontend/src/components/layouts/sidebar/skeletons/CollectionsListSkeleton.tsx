import {
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    Skeleton,
} from "@mui/material";

export const CollectionsListSkeleton = () => (
    <List disablePadding>
        {[1, 2, 3].map((i) => (
            <ListItemButton key={i} sx={{ px: 2, py: 1 }}>
                <ListItemIcon sx={{ minWidth: 36 }}>
                    <Skeleton variant="circular" width={20} height={20} />
                </ListItemIcon>
                <ListItemIcon sx={{ minWidth: 32 }}>
                    <Skeleton variant="circular" width={20} height={20} />
                </ListItemIcon>
                <ListItemText
                    primary={<Skeleton variant="text" width="70%" />}
                />
                <Skeleton variant="text" width={16} />
            </ListItemButton>
        ))}
    </List>
);
