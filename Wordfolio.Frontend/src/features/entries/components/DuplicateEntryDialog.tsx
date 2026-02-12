import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Drawer,
    Typography,
    Accordion,
    AccordionSummary,
    AccordionDetails,
    Button,
    useMediaQuery,
    useTheme,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";

import type { EntryResponse } from "../../../api/entryTypes";
import { mapEntry } from "../../../api/entryMappers";
import { EntryDetailContent } from "./EntryDetailContent";
import styles from "./DuplicateEntryDialog.module.scss";

interface DuplicateEntryDialogProps {
    readonly isOpen: boolean;
    readonly existingEntry: EntryResponse;
    readonly onCancel: () => void;
    readonly onConfirm: () => void;
}

export const DuplicateEntryDialog = ({
    isOpen,
    existingEntry,
    onCancel,
    onConfirm,
}: DuplicateEntryDialogProps) => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down("md"));

    const content = (
        <>
            <DialogTitle>Already in Vocabulary</DialogTitle>
            <DialogContent>
                <Typography sx={{ mb: 2 }}>
                    A matching entry already exists in this vocabulary.
                </Typography>
                <Accordion>
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography fontWeight="medium">
                            Existing entry details
                        </Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        <EntryDetailContent entry={mapEntry(existingEntry)} />
                    </AccordionDetails>
                </Accordion>
            </DialogContent>
            <DialogActions>
                <Button onClick={onCancel}>Cancel</Button>
                <Button variant="contained" color="primary" onClick={onConfirm}>
                    Add Anyway
                </Button>
            </DialogActions>
        </>
    );

    if (isMobile) {
        return (
            <Drawer
                anchor="bottom"
                open={isOpen}
                onClose={onCancel}
                PaperProps={{ className: styles.mobileDrawer }}
            >
                {content}
            </Drawer>
        );
    }

    return (
        <Dialog
            open={isOpen}
            onClose={onCancel}
            maxWidth="md"
            fullWidth
            PaperProps={{ className: styles.dialog }}
        >
            {content}
        </Dialog>
    );
};
