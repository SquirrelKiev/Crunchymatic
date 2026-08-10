import {type CheckPayload} from "./api";

export type Verdict = { pipeline: string, typesetting: string };

export class CheckSession {
    readonly payload: CheckPayload;

    selected = $state(0);
    verdicts = $state<Record<string, Verdict>>({});

    current = $derived.by(() => this.payload.subtitles[this.selected]);
    doneCount = $derived(Object.keys(this.verdicts).length);

    constructor(payload: CheckPayload) {
        this.payload = payload;
    }

    select(index: number) {
        this.selected = index;
    }

    setVerdict(language: string, verdict: Verdict) {
        this.verdicts[language] = verdict;
    }
}