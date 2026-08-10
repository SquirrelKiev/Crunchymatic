<script lang="ts">
    import {type CheckPayload, fetchCheck} from "./lib/api";
    import CheckView from "./lib/CheckView.svelte";

    let {checkId}: { checkId: number } = $props();

    let load = $state.raw<Promise<CheckPayload>>()

    // probably not necessary given that checkId will never change, but svelte complained, and I am but a lowly servant to the compiler
    $effect(() => {
        const abort = new AbortController()
        load = fetchCheck(checkId, abort.signal);
        return () => abort.abort();
    })
</script>

{#snippet loading()}
<p class="p-8">Loading…</p>
{/snippet}

{#if load}
    {#await load}
        {@render loading()}
    {:then payload}
        <CheckView {payload} />
    {:catch error}
        <p class="p-8 text-red-400">Failed to load: {error}</p>
    {/await}
{:else}
    {@render loading()}
{/if}