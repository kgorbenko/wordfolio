export { EntryForm } from "./components/EntryForm";
export type { EntryFormHandle, EntryFormProps } from "./components/EntryForm";
export { EntryLookupForm } from "./components/EntryLookupForm";
export { WordEntrySheet } from "./components/WordEntrySheet";

export { CreateEntryPage } from "./pages/CreateEntryPage";
export { EditEntryPage } from "./pages/EditEntryPage";
export { EntryDetailPage } from "./pages/EntryDetailPage";

export { useWordLookup } from "./hooks/useWordLookup";
export { useEntriesQuery } from "./hooks/useEntriesQuery";
export { useEntryQuery } from "./hooks/useEntryQuery";
export { useCreateEntryMutation } from "./hooks/useCreateEntryMutation";
export { useUpdateEntryMutation } from "./hooks/useUpdateEntryMutation";
export { useDeleteEntryMutation } from "./hooks/useDeleteEntryMutation";

export type {
    Entry,
    Definition,
    Translation,
    Example,
    EntryFormValues,
    EntryFormOutput,
    DefinitionItem,
    TranslationItem,
    ExampleItem,
    AnnotatedItemColor,
    LookupDefinition,
    LookupTranslation,
    LookupTranslationExample,
    WordLookupResult,
    LookupState,
    UseWordLookupResult,
} from "./types";
