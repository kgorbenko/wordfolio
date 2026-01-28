import { Entry } from "../types";
import { AnnotatedSection } from "./AnnotatedSection";
import { AnnotatedItemCard } from "./AnnotatedItemCard";
import { EntryFooter } from "./EntryFooter";

interface EntryDetailContentProps {
    readonly entry: Entry;
}

export const EntryDetailContent = ({ entry }: EntryDetailContentProps) => (
    <>
        {entry.definitions.length > 0 && (
            <AnnotatedSection title="Definitions" color="primary">
                {entry.definitions.map((def, index) => (
                    <AnnotatedItemCard
                        key={def.id}
                        index={index}
                        text={def.definitionText}
                        examples={def.examples}
                        color="primary"
                    />
                ))}
            </AnnotatedSection>
        )}

        {entry.translations.length > 0 && (
            <AnnotatedSection title="Translations" color="secondary">
                {entry.translations.map((trans, index) => (
                    <AnnotatedItemCard
                        key={trans.id}
                        index={index}
                        text={trans.translationText}
                        examples={trans.examples}
                        color="secondary"
                    />
                ))}
            </AnnotatedSection>
        )}

        <EntryFooter createdAt={entry.createdAt} updatedAt={entry.updatedAt} />
    </>
);
