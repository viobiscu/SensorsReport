// ToolbarManager.js
// ToolbarManager - Manages toolbar creation, rendering, and button actions

class ToolbarManager {
    constructor(options = {}) {
        this.options = options;
        this.toolbar = document.createElement('div');
        this.toolbar.className = options.className || 'toolbar-manager';
        this.buttons = [];
    }

    /**
     * Add a button to the toolbar
     * @param {Object} config Button configuration (icon, label, onClick, etc.)
     */
    addButton(config) {
        const button = document.createElement('button');
        if (config.icon) {
            const icon = document.createElement('i');
            icon.className = config.icon;
            button.appendChild(icon);
        }
        if (config.label) {
            let label = config.label;
            // Aggressively abbreviate common terms
            const replacements = [
                [/Data\s*Product/gi, 'DP'],
                [/Quantum\s*Leap/gi, 'QL'],
                [/Subscription/gi, 'Sub'],
                [/Entity/gi, 'Ent'],
                [/Relationship/gi, 'Rel'],
                [/Attribute/gi, 'Attr'],
                [/Table/gi, 'Tbl'],
                [/Toolbar/gi, 'TBar'],
                [/Editor/gi, 'Ed'],
                [/All/gi, 'A'],
                [/Form/gi, 'F'],
                [/View/gi, 'Vw'],
                [/Manager/gi, 'Mgr']
            ];
            let prev;
            do {
                prev = label;
                for (const [pattern, abbr] of replacements) {
                    label = label.replace(pattern, abbr);
                }
            } while (label !== prev);
            // If still too long, abbreviate each word to first 2 chars, then 1 char if needed
            if (label.length > 20) {
                let words = label.split(/\s|_|-/);
                label = words.map(w => w.slice(0, 2)).join('');
                if (label.length > 20) {
                    label = words.map(w => w[0]).join('');
                }
            }
            // If still too long, remove all vowels except first char of each word
            if (label.length > 20) {
                let words = label.split(/\s|_|-/);
                label = words.map(w => w[0] + w.slice(1).replace(/[aeiou]/gi, ''));
                label = label.join('');
            }
            // Final truncate
            if (label.length > 20) label = label.slice(0, 20);
            const span = document.createElement('span');
            span.textContent = label;
            button.appendChild(span);
        }
        if (config.title) button.title = config.title;
        if (config.className) button.className += ' ' + config.className;
        if (config.onClick) button.addEventListener('click', config.onClick);
        if (config.style) Object.assign(button.style, config.style);
        this.toolbar.appendChild(button);
        this.buttons.push(button);
        return button;
    }

    /**
     * Render the toolbar into a container
     * @param {HTMLElement} container The container to render the toolbar into
     */
    render(container) {
        if (container && this.toolbar.parentElement !== container) {
            container.appendChild(this.toolbar);
        }
    }

    /**
     * Remove all buttons from the toolbar
     */
    clear() {
        this.toolbar.innerHTML = '';
        this.buttons = [];
    }

    /**
     * Get all buttons
     * @returns {Array} Array of button elements
     */
    getButtons() {
        return this.buttons;
    }
}

export default ToolbarManager;
