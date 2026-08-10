import hljs from 'highlight.js/lib/core';
import hljsAss from 'highlightjs-ass';
import 'highlight.js/styles/felipec.css';

hljs.registerLanguage('ass', hljsAss);

export function highlightAss(source: string): string {
    return hljs.highlight(source, {language: 'ass', ignoreIllegals: true}).value;
}
