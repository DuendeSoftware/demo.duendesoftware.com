import './scss/site.scss'

// setup jquery
import $ from 'jquery';
window.$ = window.jQuery = $;

// clipboard
import ClipboardJS from './lib/clipboardjs/dist/clipboard'
window.ClipboardJS = ClipboardJS;

// import other scripts
import 'bootstrap'
import './js/signin-redirect'
import './js/site.js'