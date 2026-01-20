import { Container } from "@mui/material";
import styles from "./PageContainer.module.scss";

interface PageContainerProps {
    readonly children: React.ReactNode;
    readonly maxWidth?: number | false;
}

export const PageContainer = ({ children, maxWidth }: PageContainerProps) => (
    <Container
        maxWidth={false}
        className={styles.container}
        sx={maxWidth ? { maxWidth } : undefined}
    >
        {children}
    </Container>
);
