/**
 * JavaScript interop module for highlight.js syntax highlighting.
 * This module provides functions for syntax highlighting and clipboard operations.
 */

// Highlight.js CDN URLs - using a common subset of languages
const HLJS_CORE_URL = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js';
const HLJS_LANGUAGES = [
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/csharp.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/javascript.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/typescript.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/python.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/sql.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/json.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/xml.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/bash.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/powershell.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/yaml.min.js',
    'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/languages/css.min.js'
];

let hljsLoadPromise = null;
let hljsLoaded = false;

/**
 * Dynamically loads a script from a URL.
 * @param {string} url - The URL of the script to load.
 * @returns {Promise<void>}
 */
function loadScript(url) {
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = url;
        script.async = true;
        script.onload = () => resolve();
        script.onerror = () => reject(new Error(`Failed to load script: ${url}`));
        document.head.appendChild(script);
    });
}

/**
 * Ensures highlight.js is loaded and ready.
 * @returns {Promise<void>}
 */
async function ensureHljsLoaded() {
    if (hljsLoaded) {
        return;
    }

    if (hljsLoadPromise) {
        return hljsLoadPromise;
    }

    hljsLoadPromise = (async () => {
        // Check if hljs is already loaded globally
        if (typeof window.hljs !== 'undefined') {
            hljsLoaded = true;
            return;
        }

        // Load highlight.js core
        await loadScript(HLJS_CORE_URL);

        // Load language packs in parallel
        await Promise.all(HLJS_LANGUAGES.map(url => loadScript(url).catch(() => {
            // Silently ignore failed language loads
            console.warn(`Failed to load highlight.js language: ${url}`);
        })));

        // Configure highlight.js
        if (window.hljs) {
            window.hljs.configure({
                ignoreUnescapedHTML: true
            });
        }

        hljsLoaded = true;
    })();

    return hljsLoadPromise;
}

/**
 * Highlights a specific code element.
 * @param {HTMLElement} element - The code element to highlight.
 */
export async function highlightElement(element) {
    if (!element) {
        return;
    }

    try {
        await ensureHljsLoaded();

        if (window.hljs && element) {
            // Remove any existing highlighting
            element.classList.remove('hljs');
            element.removeAttribute('data-highlighted');

            window.hljs.highlightElement(element);
        }
    } catch (error) {
        console.warn('Highlight.js highlighting failed:', error);
    }
}

/**
 * Highlights all code elements in the document with language- class.
 */
export async function highlightAll() {
    try {
        await ensureHljsLoaded();

        if (window.hljs) {
            // Find all code blocks that haven't been highlighted yet
            const codeBlocks = document.querySelectorAll('pre code[class*="language-"]:not([data-highlighted="yes"])');
            codeBlocks.forEach(block => {
                window.hljs.highlightElement(block);
            });
        }
    } catch (error) {
        console.warn('Highlight.js highlighting failed:', error);
    }
}

/**
 * Copies text to the clipboard.
 * @param {string} text - The text to copy.
 * @returns {Promise<boolean>} - True if copy succeeded, false otherwise.
 */
export async function copyToClipboard(text) {
    if (!text) {
        return false;
    }

    try {
        // Modern clipboard API
        if (navigator.clipboard && navigator.clipboard.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        }

        // Fallback for older browsers
        const textarea = document.createElement('textarea');
        textarea.value = text;
        textarea.style.position = 'fixed';
        textarea.style.left = '-9999px';
        textarea.style.top = '-9999px';
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();

        let success = false;
        try {
            success = document.execCommand('copy');
        } finally {
            document.body.removeChild(textarea);
        }

        return success;
    } catch (error) {
        console.error('Copy to clipboard failed:', error);
        return false;
    }
}

/**
 * Gets the current theme preference (light or dark).
 * @returns {string} - 'dark' or 'light'
 */
export function getThemePreference() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'dark';
    }
    return 'light';
}

/**
 * Registers a callback for theme changes.
 * @param {function} callback - Function to call when theme changes.
 * @returns {function} - Function to unregister the callback.
 */
export function onThemeChange(callback) {
    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

    const handler = (e) => {
        callback(e.matches ? 'dark' : 'light');
    };

    mediaQuery.addEventListener('change', handler);

    return () => mediaQuery.removeEventListener('change', handler);
}
