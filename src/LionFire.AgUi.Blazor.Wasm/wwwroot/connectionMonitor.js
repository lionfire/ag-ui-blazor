// Connection monitor JavaScript module for Blazor WASM
// Provides browser connectivity detection using Navigator.onLine API

let dotNetHelper = null;

/**
 * Initializes the connection monitor.
 * @param {DotNetObjectReference} dotNetRef - Reference to the .NET ConnectionMonitor instance.
 * @returns {boolean} - The current online status.
 */
export function initialize(dotNetRef) {
    dotNetHelper = dotNetRef;

    // Set up event listeners for online/offline events
    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    // Return current online status
    return navigator.onLine;
}

/**
 * Checks the current online status.
 * @returns {boolean} - The current online status.
 */
export function checkOnline() {
    return navigator.onLine;
}

/**
 * Handles the browser coming online.
 */
function handleOnline() {
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnOnlineStateChanged', true);
    }
}

/**
 * Handles the browser going offline.
 */
function handleOffline() {
    if (dotNetHelper) {
        dotNetHelper.invokeMethodAsync('OnOnlineStateChanged', false);
    }
}

/**
 * Disposes the connection monitor and removes event listeners.
 */
export function dispose() {
    window.removeEventListener('online', handleOnline);
    window.removeEventListener('offline', handleOffline);
    dotNetHelper = null;
}
