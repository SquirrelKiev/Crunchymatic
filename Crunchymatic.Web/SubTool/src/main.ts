import {mount} from 'svelte';
// import 'normalize.css/normalize.css';
import './app.css';
import App from './App.svelte';

const target = document.getElementById('subtool')!;
const checkId = Number(target.dataset.checkId);
if (!Number.isInteger(checkId)) {
    throw new Error('Invalid check ID');
}

const app = mount(App, {
    target: target,
    props: {checkId},
});

export default app;
