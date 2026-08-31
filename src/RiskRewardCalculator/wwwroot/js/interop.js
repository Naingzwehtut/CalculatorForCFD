// interop.js
//
// WHY THIS FILE EXISTS:
// Blazor WebAssembly runs compiled .NET IL through the browser's WebAssembly
// engine. That gives .NET access to CPU-like computation, but the WebAssembly
// sandbox has NO built-in bindings to browser-only APIs such as the Clipboard
// API or window.localStorage - those only exist as JavaScript objects on
// `window`/`navigator`. To reach them, .NET code calls into a small amount of
// JavaScript through Blazor's JS interop bridge (IJSRuntime). Everything else
// in this app (all the math, all the validation, all the UI logic) is plain
// C# - this file is intentionally the ONLY JavaScript in the project.
window.riskRewardInterop = {
    copyToClipboard: async function (text) {
        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(text);
                return true;
            }
        } catch {
            // fall through to the legacy fallback below
        }

        // Fallback for browsers/contexts without the async Clipboard API
        // (e.g. non-HTTPS local testing).
        const textarea = document.createElement("textarea");
        textarea.value = text;
        textarea.style.position = "fixed";
        textarea.style.opacity = "0";
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();
        let success = false;
        try {
            success = document.execCommand("copy");
        } catch {
            success = false;
        }
        document.body.removeChild(textarea);
        return success;
    },

    localStorageGet: function (key) {
        return window.localStorage.getItem(key);
    },

    localStorageSet: function (key, value) {
        window.localStorage.setItem(key, value);
    },

    localStorageRemove: function (key) {
        window.localStorage.removeItem(key);
    }
};
