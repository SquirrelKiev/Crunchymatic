export type Subtitle = {
    languageCode: string;
    fileName: string;
    // TODO: these should really be enums or something
    autoPipeline: string;
    autoTypesetting: string;
    content: string;
}

export type CheckPayload = {
    animeTitle: string;
    episode: string;
    subtitlesGrabbed: Date;
    subtitles: Subtitle[];
}

// I love weakly typed languages!!
type CheckPayloadRaw = Omit<CheckPayload, 'subtitlesGrabbed'> & { subtitlesGrabbed: string };

export async function fetchCheck(checkId: number, signal: AbortSignal): Promise<CheckPayload> {
    const res = await fetch(`/api/check/${checkId}`, { signal });
    if(!res.ok)
        throw new Error(`Failed to fetch check with id ${checkId}: HTTP ${res.status}`);
    const raw: CheckPayloadRaw = await res.json();
    return { ...raw, subtitlesGrabbed: new Date(raw.subtitlesGrabbed) };
}
