/**
 * Button Editor Component
 * This provides consistent button creation and styling functionality
 * that can be used across the entire application.
 */

class ButtonEditor {
    /**
     * Create a styled button with common properties
     * @param {Object} config Button configuration
     * @param {string} [config.title] Button tooltip
     * @param {string} [config.icon] FontAwesome icon class
     * @param {string} [config.text] Button text content
     * @param {string} [config.color] Background color (for operation buttons)
     * @param {boolean} [config.isOperation=false] Whether this is an operation button
     * @returns {HTMLButtonElement} The created button
     */
    createButton(config) {
        const button = document.createElement('button');
        
        if (config.title) {
            button.title = config.title;
        }

        let iconElement = null; // Keep a reference to the icon
        if (config.icon) {
            iconElement = document.createElement('i');
            iconElement.className = config.icon;
            button.appendChild(iconElement);
        }

        if (config.text) {
            if (config.icon) {
                // Add spacing between icon and text
                const span = document.createElement('span');
                span.style.marginLeft = '4px';
                span.textContent = config.text;
                button.appendChild(span);
            } else {
                button.textContent = config.text;
            }
        }

        if (config.isOperation) {
            this.applyOperationStyle(button, config.color, iconElement);
        } else {
            this.applyDefaultStyle(button, iconElement);
        }

        return button;
    }

    /**
     * Apply default button styling
     * @param {HTMLButtonElement} button The button to style
     * @param {HTMLElement | null} iconElement The icon element, if any
     */
    applyDefaultStyle(button, iconElement) {
        button.style.backgroundColor = 'var(--button-bg)';
        button.style.border = '1px solid var(--button-border)';
        button.style.color = 'var(--button-text)'; // For button text
        button.style.borderRadius = '3px';
        button.style.padding = '4px 8px';
        button.style.margin = '0 3px';
        button.style.fontSize = '12px';
        button.style.cursor = 'pointer';
        button.style.display = 'flex';
        button.style.alignItems = 'center';
        button.style.justifyContent = 'center';
        button.style.outline = 'none';
        button.style.minWidth = '28px';
        button.style.height = '28px';
        
        if (iconElement) {
            iconElement.style.color = 'var(--button-icon-color)';
        }

        button.addEventListener('mouseover', () => {
            button.style.backgroundColor = 'var(--button-hover-bg)';
            button.style.borderColor = 'var(--button-hover-border)';
            button.style.color = 'var(--button-hover-text)';
            if (iconElement) {
                iconElement.style.color = 'var(--button-hover-text)'; // Icon uses hover text color
            }
        });
        
        button.addEventListener('mouseout', () => {
            button.style.backgroundColor = 'var(--button-bg)';
            button.style.borderColor = 'var(--button-border)';
            button.style.color = 'var(--button-text)';
            if (iconElement) {
                iconElement.style.color = 'var(--button-icon-color)';
            }
        });
    }

    /**
     * Apply operation button styling with colors
     * @param {HTMLButtonElement} button The button to style
     * @param {string} color The background color
     * @param {HTMLElement | null} iconElement The icon element, if any
     */
    applyOperationStyle(button, color, iconElement) {
        button.style.backgroundColor = color;
        button.style.border = '1px solid ' + color;
        button.style.borderRadius = '3px';
        button.style.padding = '4px 8px';
        button.style.margin = '0 3px';
        button.style.fontSize = '12px';
        button.style.cursor = 'pointer';
        button.style.display = 'inline-flex'; // Changed from flex to inline-flex for consistency
        button.style.alignItems = 'center';
        button.style.justifyContent = 'center';
        button.style.color = '#fff'; // Operation buttons use white text/icon for contrast
        button.style.outline = 'none';
        button.style.minWidth = '28px';
        button.style.height = '28px';

        if (iconElement) {
            iconElement.style.color = '#fff';
        }
        
        button.addEventListener('mouseover', () => {
            const hoverColor = this.darkenColor(color, 10);
            button.style.backgroundColor = hoverColor;
            button.style.borderColor = hoverColor;
            // Text and icon color remain #fff on hover for operation buttons
        });
        
        button.addEventListener('mouseout', () => {
            button.style.backgroundColor = color;
            button.style.borderColor = color;
            // Text and icon color remain #fff for operation buttons
        });
    }

    /**
     * Create a separator div for the toolbar
     * @returns {HTMLDivElement} The separator element
     */
    createSeparator() {
        const separator = document.createElement('div');
        separator.style.borderLeft = '1px solid var(--border-color)';
        separator.style.height = '20px';
        separator.style.margin = '0 8px';
        return separator;
    }

    /**
     * Darken a hex color by a percentage
     * @param {string} color The hex color to darken
     * @param {number} percent The percentage to darken by
     * @returns {string} The darkened color
     */
    darkenColor(color, percent) {
        // Ensure color is a string and starts with #
        if (typeof color !== 'string' || !color.startsWith('#')) {
            console.warn('darkenColor: Invalid color format. Expected hex string starting with #. Received:', color);
            return color; // Return original color if format is invalid
        }
        const num = parseInt(color.replace('#', ''), 16);
        const amt = Math.round(2.55 * percent);
        const R = (num >> 16) - amt;
        const G = (num >> 8 & 0x00FF) - amt;
        const B = (num & 0x0000FF) - amt;
        return '#' + (1 << 24 | (R < 255 ? (R < 0 ? 0 : R) : 255) << 16 | (G < 255 ? (G < 0 ? 0 : G) : 255) << 8 | (B < 255 ? (B < 0 ? 0 : B) : 255)).toString(16).slice(1);
    }
}

// Add module exports
window.ButtonEditor = ButtonEditor;
export default ButtonEditor;