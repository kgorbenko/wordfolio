import {
    DialogTitle,
    DialogContent,
    DialogActions,
    Typography,
    Accordion,
    AccordionSummary,
    AccordionDetails,
    Button,
} from "@mui/material";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";

import type { Entry } from "../../api/types/entries";
import { EntryDetailContent } from "./EntryDetailContent";
import { ResponsiveDialog } from "../ResponsiveDialog";
import styles from "./DuplicateEntryDialog.module.scss";

interface DuplicateEntryDialogProps {
    readonly isOpen: boolean;
    readonly existingEntry: Entry;
    readonly onCancel: () => void;
    readonly onConfirm: () => void;
}

export const DuplicateEntryDialog = ({
    isOpen,
    existingEntry,
    onCancel,
    onConfirm,
}: DuplicateEntryDialogProps) => {
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
                        <EntryDetailContent entry={existingEntry} />
                    </AccordionDetails>
                </Accordion>
            </DialogContent>
            <DialogActions>
                <Button variant="outlined" onClick={onCancel}>
                    Cancel
                </Button>
                <Button variant="contained" color="primary" onClick={onConfirm}>
                    Add Anyway
                </Button>
            </DialogActions>
        </>
    );

    return (
        <ResponsiveDialog
            open={isOpen}
            onClose={onCancel}
            maxWidth="md"
            fullWidth
            dialogPaperClassName={styles.dialog}
        >
            {content}
        </ResponsiveDialog>
    );
};
