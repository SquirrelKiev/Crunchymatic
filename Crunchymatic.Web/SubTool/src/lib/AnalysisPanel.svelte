<script lang="ts">
    import type { CheckSession} from "./check.svelte";
    
    let {session} : {session: CheckSession} = $props();
</script>
<div class="w-96 shrink-0 border-l-2 border-white">
    {#if session.current}
        <div>
            <h2 class="mb-2 text-2xl p-4 font-bold">Automation says</h2>
            <div class="flex">
                <div class="flex-1 border-2 border-r">
                    <p class="text-md font-bold">Pipeline</p>
                    <p class="text-lg font-bold">{session.current.autoPipeline}</p>
                </div>
                <div class="flex-1 border-2 border-l">
                    <p class="text-md font-bold">Typesetting</p>
                    <p class="text-lg font-bold">{session.current.autoTypesetting}</p>
                </div>
            </div>
        </div>

        <!-- TODO: the actual classification stuff -->
    {/if}
</div>