// Message input keyboard handling
export function initializeMessageInput(element, dotNetRef) {
    if (!element) return;

    // MudTextField renders input inside nested divs - find it
    const input = element.querySelector('input, textarea');
    if (!input) {
        console.warn('MudMessageInput: Could not find input element');
        return;
    }

    // Store reference for clearing
    element._mudInput = input;
    element._dotNetRef = dotNetRef;

    input.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            dotNetRef.invokeMethodAsync('HandleEnterPressed');
        }
    });
}

export function clearInput(element) {
    if (!element) return;

    const input = element._mudInput || element.querySelector('input, textarea');
    if (input) {
        input.value = '';
        // Trigger input event so Blazor binding updates
        input.dispatchEvent(new Event('input', { bubbles: true }));
        input.focus();
    }
}

export function dispose(element) {
    if (element) {
        element._mudInput = null;
        element._dotNetRef = null;
    }
}
