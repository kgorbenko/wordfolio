module Wordfolio.Api.Api.Dictionary.Mappers

[<Literal>]
let JsonDelimiter = "---JSON---"

let createWordLookupPrompt(text: string) =
    $"""For the English word or phrase "{text}", provide definitions and Russian translations.

Output in two parts separated by the exact marker {JsonDelimiter}

First part - clean readable text for end users. Format:

[part of speech] Definition text here.
"Example sentence with *{text}* highlighted."

[part of speech] Перевод на русский
RU: "Пример на русском с *переводом*."
EN: "English translation of the example."

For phrases, omit [part of speech]:
Definition of the phrase.
"Example with *the phrase* in use."

{JsonDelimiter}

Second part - raw JSON only (no code fences):
{{"definitions":[{{"definition":"...","partOfSpeech":"verb|noun|adj|adv|null","exampleSentences":["..."]}}],"translations":[{{"translation":"...","partOfSpeech":"verb|noun|adj|adv|null","examples":[{{"russian":"...","english":"..."}}]}}]}}

Rules:
- Provide the most common definitions and translations (up to 5 each)
- For single words, include partOfSpeech (verb, noun, adj, adv)
- For phrases, set partOfSpeech to null and omit [part of speech] prefix
- Definitions under 10 words each
- Example sentences under 15 words each
- 1-2 examples per definition/translation
- Highlight "{text}" with asterisks in examples (e.g., *{text}*)
- No markdown formatting, no headers, no extra text
- Blank line between each definition and translation entry
- JSON must be valid and compact (single line, no pretty-printing)"""
