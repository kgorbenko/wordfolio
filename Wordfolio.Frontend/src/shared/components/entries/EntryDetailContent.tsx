import type { Entry } from "../../api/types/entries";
import { AnnotatedSection } from "./AnnotatedSection";
import { AnnotatedItemCard } from "./AnnotatedItemCard";

interface EntryDetailContentProps {
    readonly entry: Entry;
}

export const EntryDetailContent = ({ entry }: EntryDetailContentProps) => (
    <>
        {entry.definitions.length > 0 && (
            <AnnotatedSection title="Definitions" color="primary">
                {entry.definitions.map((def) => (
                    <AnnotatedItemCard
                        key={def.id}
                        text={def.definitionText}
                        examples={def.examples}
                    />
                ))}
            </AnnotatedSection>
        )}

        {entry.translations.length > 0 && (
            <AnnotatedSection title="Translations" color="secondary">
                {entry.translations.map((trans) => (
                    <AnnotatedItemCard
                        key={trans.id}
                        text={trans.translationText}
                        examples={trans.examples}
                    />
                ))}
            </AnnotatedSection>
        )}
    </>
);
