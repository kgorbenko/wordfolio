import { Breadcrumbs, Typography } from "@mui/material";
import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { Link } from "@tanstack/react-router";
import styles from "./BreadcrumbNav.module.scss";

export interface BreadcrumbItem {
    readonly label: string;
    readonly to?: string;
    readonly params?: Record<string, string>;
}

interface BreadcrumbNavProps {
    readonly items: BreadcrumbItem[];
}

export const BreadcrumbNav = ({ items }: BreadcrumbNavProps) => (
    <div className={styles.breadcrumbs}>
        <Breadcrumbs separator={<NavigateNextIcon fontSize="small" />}>
            {items.map((item, index) =>
                item.to ? (
                    <Link
                        key={index}
                        to={item.to}
                        params={item.params}
                        className={styles.link}
                    >
                        <Typography
                            color="text.secondary"
                            className={styles.linkText}
                        >
                            {item.label}
                        </Typography>
                    </Link>
                ) : (
                    <Typography
                        key={index}
                        color="text.primary"
                        fontWeight={500}
                    >
                        {item.label}
                    </Typography>
                )
            )}
        </Breadcrumbs>
    </div>
);
