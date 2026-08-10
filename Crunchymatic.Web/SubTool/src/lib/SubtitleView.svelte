<script lang="ts">
    import type { Subtitle } from "./api";
    
    let {subtitle}: {subtitle: Subtitle | undefined } = $props();
</script>
<div class="flex-1 min-w-0 overflow-auto">
    {#if subtitle}
        <!-- TODO: hook up highlight.js to this -->
        <pre class="p-4 text-xs whitespace-pre"><code>{subtitle.content}</code></pre>
    {/if}
</div>