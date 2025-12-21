/**
 * JavaScript interop module for MudMessageList scroll management.
 * Provides auto-scroll detection and scroll-to-bottom functionality.
 */

// WeakMap to store scroll listeners for cleanup
const scrollListeners = new WeakMap();

/**
 * Checks if an element is scrolled to the bottom (within a small threshold).
 * @param {HTMLElement} element - The scrollable element.
 * @returns {boolean} - True if the element is at or near the bottom.
 */
function isAtBottom(element) {
    if (!element) {
        return true;
    }
    // Allow for a small threshold (5px) to account for rendering differences
    const threshold = 5;
    return element.scrollHeight - element.scrollTop - element.clientHeight <= threshold;
}

/**
 * Scrolls an element to the bottom.
 * @param {HTMLElement} element - The element to scroll.
 */
export function scrollToBottom(element) {
    if (!element) {
        console.warn('scrollToBottom: no element provided');
        return;
    }

    // Use requestAnimationFrame to ensure DOM has updated
    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            console.log('scrollToBottom called:', {
                scrollHeight: element.scrollHeight,
                scrollTop: element.scrollTop,
                clientHeight: element.clientHeight
            });
            element.scrollTop = element.scrollHeight;
            console.log('scrollToBottom after:', { scrollTop: element.scrollTop });
        });
    });
}

/**
 * Initializes a scroll listener on an element that notifies .NET when scroll position changes.
 * @param {HTMLElement} element - The scrollable element.
 * @param {object} dotNetRef - The .NET object reference for callbacks.
 */
export function initScrollListener(element, dotNetRef) {
    if (!element || !dotNetRef) {
        console.debug('initScrollListener: missing element or dotNetRef');
        return;
    }

    console.debug('initScrollListener: setting up scroll listener');

    // Remove any existing listener
    removeScrollListener(element);

    let ticking = false;

    const scrollHandler = () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                try {
                    dotNetRef.invokeMethodAsync('OnScroll', isAtBottom(element));
                } catch (e) {
                    // .NET reference may have been disposed
                    console.debug('Scroll callback failed:', e);
                }
                ticking = false;
            });
            ticking = true;
        }
    };

    element.addEventListener('scroll', scrollHandler, { passive: true });
    scrollListeners.set(element, scrollHandler);
}

/**
 * Removes the scroll listener from an element.
 * @param {HTMLElement} element - The element to remove the listener from.
 */
export function removeScrollListener(element) {
    if (!element) {
        return;
    }

    const handler = scrollListeners.get(element);
    if (handler) {
        element.removeEventListener('scroll', handler);
        scrollListeners.delete(element);
    }
}

/**
 * Scrolls an element to the bottom smoothly with animation.
 * @param {HTMLElement} element - The element to scroll.
 */
export function scrollToBottomSmooth(element) {
    if (!element) {
        return;
    }
    element.scrollTo({
        top: element.scrollHeight,
        behavior: 'smooth'
    });
}

/**
 * Gets the current scroll position information.
 * @param {HTMLElement} element - The scrollable element.
 * @returns {object} - Object containing scrollTop, scrollHeight, clientHeight, and isAtBottom.
 */
export function getScrollInfo(element) {
    if (!element) {
        return { scrollTop: 0, scrollHeight: 0, clientHeight: 0, isAtBottom: true };
    }
    return {
        scrollTop: element.scrollTop,
        scrollHeight: element.scrollHeight,
        clientHeight: element.clientHeight,
        isAtBottom: isAtBottom(element)
    };
}
