<script lang="ts">
    import type { Subtitle } from "./api";
    import { highlightAss } from "./highlight";

    let {subtitle}: {subtitle: Subtitle | undefined } = $props();

    const highlighted = $derived(subtitle ? highlightAss(subtitle.content) : '');
</script>
<div class="flex-1 min-w-0 overflow-auto">
    {#if subtitle}
        <pre class="text-xs"><code class="hljs language-ass">{@html highlighted}</code></pre>
    {/if}
</div>