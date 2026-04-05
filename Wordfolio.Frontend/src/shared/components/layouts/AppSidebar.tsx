import {
    Box,
    Button,
    Collapse,
    Divider,
    Drawer,
    IconButton,
    Link as MuiLink,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    ListSubheader,
    Typography,
} from "@mui/material";
import DescriptionOutlinedIcon from "@mui/icons-material/DescriptionOutlined";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import FolderIcon from "@mui/icons-material/Folder";
import LogoutIcon from "@mui/icons-material/Logout";

import classnames from "classnames";

import styles from "./AppSidebar.module.scss";
import { WordfolioBrand } from "./WordfolioBrand";

export interface NavCollection {
    readonly id: number;
    readonly name: string;
    readonly entryCount: number;
    readonly active?: boolean;
    readonly expanded?: boolean;
    readonly activeChildId?: number;
    readonly children?: {
        readonly id: number;
        readonly name: string;
        readonly entryCount: number;
    }[];
    readonly onClick?: () => void;
    readonly onExpand?: () => void;
    readonly onChildClick?: (id: number) => void;
}

export interface NavUser {
    readonly initials: string;
    readonly email: string;
    readonly onLogout?: () => void;
}

const ChildItem = ({
    name,
    entryCount,
    active,
    onClick,
}: {
    name: string;
    entryCount: number;
    active?: boolean;
    onClick?: () => void;
}) => (
    <ListItemButton
        onClick={onClick}
        className={styles.sidebarListItem}
        sx={active ? { color: "primary.main" } : undefined}
    >
        <ListItemText primary={name} />
        <Box
            component="span"
            className={styles.childItemCount}
            sx={{ color: "text.primary" }}
        >
            {entryCount}
        </Box>
    </ListItemButton>
);

interface SidebarContentProps {
    readonly draftCount: number;
    readonly collections: NavCollection[];
    readonly user: NavUser;
    readonly onAddEntry: () => void;
    readonly onDraftsClick: () => void;
    readonly onCreateCollection?: () => void;
    readonly showBrand?: boolean;
}

const SidebarListItem = ({
    title,
    onClick,
    onExpand,
    count,
    icon,
    active,
    expandable,
    expanded,
    className,
}: {
    title: string;
    onClick: () => void;
    onExpand?: () => void;
    count?: number;
    icon: React.ReactNode;
    active?: boolean;
    expandable?: boolean;
    expanded?: boolean;
    className?: string;
}) => (
    <ListItemButton
        onClick={onClick}
        className={classnames(styles.sidebarListItem, className)}
        sx={
            active
                ? {
                      color: "primary.main",
                      "& .MuiListItemIcon-root": { color: "primary.main" },
                      "& .MuiListItemText-primary": { color: "primary.main" },
                  }
                : undefined
        }
    >
        <ListItemIcon>{icon}</ListItemIcon>
        <ListItemText primary={title} />
        <Box
            component="span"
            className={styles.count}
            sx={{ color: "text.primary" }}
        >
            {count}
        </Box>
        {expandable && (
            <ChevronRightIcon
                className={styles.expandIcon}
                sx={{
                    color: "text.secondary",
                    transform: expanded ? "rotate(90deg)" : "rotate(0deg)",
                    transition: "transform 250ms ease",
                }}
                onClick={(e) => {
                    e.stopPropagation();
                    onExpand?.();
                }}
            />
        )}
    </ListItemButton>
);

const SidebarContent = ({
    draftCount,
    collections,
    user,
    onAddEntry,
    onDraftsClick,
    onCreateCollection,
    showBrand,
}: SidebarContentProps) => (
    <Box className={styles.sidebarContent}>
        {showBrand && (
            <>
                <Box
                    className={styles.sidebarBrand}
                    sx={{ bgcolor: "primary.main" }}
                >
                    <WordfolioBrand />
                </Box>
                <Divider />
            </>
        )}
        <Box className={styles.nav}>
            <Box className={styles.addEntryWrapper}>
                <Button variant="contained" fullWidth onClick={onAddEntry}>
                    + Add Entry
                </Button>
            </Box>

            {draftCount > 0 && (
                <>
                    <List disablePadding>
                        <SidebarListItem
                            title="Drafts"
                            onClick={onDraftsClick}
                            icon={
                                <DescriptionOutlinedIcon
                                    className={styles.navIcon}
                                />
                            }
                            count={draftCount}
                            className={styles.drafts}
                        />
                    </List>

                    <Divider className={styles.navDivider} />
                </>
            )}

            <List
                disablePadding
                subheader={
                    collections.length > 0 ? (
                        <ListSubheader
                            disableSticky
                            className={styles.collectionsSubheader}
                        >
                            Collections
                        </ListSubheader>
                    ) : undefined
                }
            >
                {collections.length === 0 && (
                    <Box className={styles.collectionsEmpty}>
                        <Typography variant="body2" color="text.secondary">
                            <MuiLink
                                component="button"
                                variant="body2"
                                onClick={onCreateCollection}
                            >
                                Create
                            </MuiLink>{" "}
                            collection to get started.
                        </Typography>
                    </Box>
                )}
                {collections.map((collection) => {
                    const hasChildren =
                        collection.children && collection.children.length > 0;
                    return (
                        <Box
                            key={collection.id}
                            className={classnames(styles.collectionItem, {
                                [styles.collectionGroup]: hasChildren,
                            })}
                            sx={(theme) => ({
                                borderRadius: `${theme.shape.borderRadius}px`,
                                ...(collection.expanded
                                    ? { bgcolor: "action.listSelected" }
                                    : {}),
                            })}
                        >
                            <SidebarListItem
                                title={collection.name}
                                onClick={collection.onClick ?? (() => {})}
                                onExpand={collection.onExpand}
                                icon={<FolderIcon className={styles.navIcon} />}
                                active={collection.active}
                                expandable={hasChildren}
                                expanded={collection.expanded}
                            />

                            {hasChildren && (
                                <Collapse in={collection.expanded}>
                                    <Box
                                        className={styles.childrenTree}
                                        sx={{ borderColor: "divider" }}
                                    >
                                        {collection.children!.map((child) => (
                                            <ChildItem
                                                key={child.id}
                                                name={child.name}
                                                entryCount={child.entryCount}
                                                active={
                                                    child.id ===
                                                    collection.activeChildId
                                                }
                                                onClick={() =>
                                                    collection.onChildClick?.(
                                                        child.id
                                                    )
                                                }
                                            />
                                        ))}
                                    </Box>
                                </Collapse>
                            )}
                        </Box>
                    );
                })}
            </List>
        </Box>

        <Divider />

        <Box className={styles.userSection}>
            <Box
                className={styles.avatar}
                sx={{
                    bgcolor: "primary.main",
                }}
            >
                <Box
                    component="span"
                    className={styles.avatarText}
                    sx={{ color: "text.topbarPrimary" }}
                >
                    {user.initials}
                </Box>
            </Box>
            <Typography
                variant="body2"
                color="text.primary"
                noWrap
                className={styles.userEmail}
            >
                {user.email}
            </Typography>
            <IconButton
                size="small"
                sx={{ color: "text.secondary" }}
                onClick={user.onLogout}
            >
                <LogoutIcon className={styles.logoutIcon} />
            </IconButton>
        </Box>
    </Box>
);

interface AppSidebarProps {
    readonly variant: "permanent" | "temporary";
    readonly open?: boolean;
    readonly onClose?: () => void;
    readonly draftCount: number;
    readonly collections: NavCollection[];
    readonly user: NavUser;
    readonly onAddEntry: () => void;
    readonly onDraftsClick: () => void;
    readonly onCreateCollection?: () => void;
}

export const AppSidebar = ({
    variant,
    open,
    onClose,
    draftCount,
    collections,
    user,
    onAddEntry,
    onDraftsClick,
    onCreateCollection,
}: AppSidebarProps) => {
    const contentProps = {
        draftCount,
        collections,
        user,
        onAddEntry,
        onDraftsClick,
        onCreateCollection,
    };

    if (variant === "temporary") {
        return (
            <Drawer
                variant="temporary"
                open={open}
                onClose={onClose}
                ModalProps={{ keepMounted: true }}
            >
                <SidebarContent {...contentProps} showBrand />
            </Drawer>
        );
    }

    return (
        <Drawer variant="permanent">
            <SidebarContent {...contentProps} />
        </Drawer>
    );
};
