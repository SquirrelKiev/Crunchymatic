<script lang="ts">
    import type {CheckPayload} from "./api";
    import {CheckSession} from "./check.svelte";
    import SubtitleView from "./SubtitleView.svelte";
    import LanguageList from "./LanguageList.svelte";
    import AnalysisPanel from "./AnalysisPanel.svelte";
    
    let {payload}: {payload: CheckPayload} = $props();
    
    const session: CheckSession = $derived(new CheckSession(payload)); // probably not ideal being derived
    const subtitlesGrabbed = $derived.by(() => {
        return session.payload.subtitlesGrabbed.toISOString().split('T')[0];
    });
</script>
<div class="flex flex-col h-full">
        <div class="flex items-center gap-8 px-8 py-2 border-b-2 border-white">
            <h1 class="text-lg font-bold">{session.payload.animeTitle}</h1>
            <h2 class="text-lg font-bold">Episode {session.payload.episode} :: grabbed {subtitlesGrabbed}</h2>
            <span class="text-lg font-bold ml-auto">{session.doneCount}/{session.payload.subtitles.length} done</span>
        </div>
        <div class="flex flex-1 min-h-0">
            <LanguageList session={session}/>
            <SubtitleView subtitle={session.current}/>
            <AnalysisPanel session={session}/>
        </div>
    </div>