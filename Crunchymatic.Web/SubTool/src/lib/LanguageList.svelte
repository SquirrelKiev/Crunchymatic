<script lang="ts">
    import type { CheckSession} from "./check.svelte";

    let {session} : {session: CheckSession} = $props();
</script>
<div class="w-64 shrink-0 border-r-2 border-white">
    <!-- TODO: sort by the arbitrary sorting arrangement I use atm -->
    {#each session.payload.subtitles as subtitle, i (subtitle.languageCode)}
        <!-- TODO: language code here needs resolving to an actual name -->
        {@const verdict = session.verdicts[subtitle.languageCode]}
        <button class="flex w-full px-4 py-2 border-b-2 border-white text-left cursor-pointer"
        class:bg-white={i === session.selected}
        class:text-black={i === session.selected}
        onclick={() => session.select(i)}
        >
            <span>{subtitle.languageCode}</span>
            <span class="ml-auto">{verdict ? `${verdict.pipeline} / ${verdict.typesetting}` : "- / -"}</span>
        </button>
    {/each}
</div>