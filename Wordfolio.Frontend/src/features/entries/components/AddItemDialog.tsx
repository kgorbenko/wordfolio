import { useState, useEffect } from "react";
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
} from "@mui/material";

interface AddItemDialogProps {
    readonly open: boolean;
    readonly title: string;
    readonly label: string;
    readonly multiline?: boolean;
    readonly onClose: () => void;
    readonly onAdd: (text: string) => void;
}

export const AddItemDialog = ({
    open,
    title,
    label,
    multiline = false,
    onClose,
    onAdd,
}: AddItemDialogProps) => {
    const [text, setText] = useState("");

    useEffect(() => {
        if (!open) {
            setText("");
        }
    }, [open]);

    const handleAdd = () => {
        if (text.trim()) {
            onAdd(text.trim());
            setText("");
            onClose();
        }
    };

    const handleKeyDown = (event: React.KeyboardEvent) => {
        if (event.key === "Enter" && !multiline && text.trim()) {
            event.preventDefault();
            handleAdd();
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>{title}</DialogTitle>
            <DialogContent>
                <TextField
                    autoFocus
                    fullWidth
                    multiline={multiline}
                    rows={multiline ? 3 : 1}
                    label={label}
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    onKeyDown={handleKeyDown}
                    sx={{ mt: 1 }}
                />
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button
                    variant="contained"
                    onClick={handleAdd}
                    disabled={!text.trim()}
                >
                    Add
                </Button>
            </DialogActions>
        </Dialog>
    );
};
