/**
 * FormQuantumAllTableToolbarEditor Component
 * This provides specialized quantum table functionality by extending TableToolbarEditorModular
 */

import TableToolbarEditorModular from './TableToolbarEditorModular.js';

class FormQuantumAllTableToolbarEditor extends TableToolbarEditorModular {
    /**
     * Create a quantum table toolbar with specific functionality
     * @param {Object} config Configuration options
     * @param {boolean} [config.showToolbar=true] Whether to show the toolbar
     * @param {string[]} [config.columns=[]] Initial columns for the table
     * @param {boolean} [config.showSearch=true] Whether to show search functionality
     * @param {boolean} [config.showPagination=true] Whether to show pagination controls
     * @param {boolean} [config.showColumnToggle=true] Whether to show column toggle functionality
     * @param {number} [config.pageSize=10] Number of rows per page
     */
    constructor(config) {
        // Call parent constructor with enhanced configuration
        super({
            ...config,
            showToolbar: true,
            mode: 'table'  // Always start in table mode for quantum view
        });
    }

    /**
     * Override the displayTable method to add quantum-specific features
     * @param {Array} data Array of objects to display in table
     */
    displayTable(data) {
        // Only call parent implementation
        if (super.displayTable) {
            super.displayTable(data);
        }
    }

    /**
     * Set the data for the table and re-render the view
     * @param {string|Array} value - JSON string or array of objects
     */
    setValue(value) {
        console.log('[Debug] setValue called with:', value);
        try {
            if (typeof value === 'string') {
                this.data = JSON.parse(value);
            } else if (Array.isArray(value)) {
                this.data = value;
            } else {
                throw new Error('setValue expects a JSON string or array');
            }
        } catch (e) {
            console.error('[Debug] setValue error:', e);
            this.data = [];
        }
        this._render();
    }

    /**
     * Update the display (alias for _render)
     */
    updateDisplay() {
        console.log('[Debug] updateDisplay called');
        this._render();
        // Add double-click handler after rendering
        const table = this.contentEl.querySelector('table.json-table');
        console.log('[QuantumTable] table found:', !!table, table);
        if (!table) return;
        const rows = table.querySelectorAll('tbody tr');
        console.log('[QuantumTable] row count:', rows.length);
        rows.forEach((row, idx) => {
            row.addEventListener('dblclick', (event) => {
                console.debug('[QuantumTable] Double-click event captured on row', idx, row);
                // If user is selecting text, do not trigger entity view
                const selection = window.getSelection();
                if (selection && selection.toString().length > 0) {
                    console.debug('[QuantumTable] Text selection detected, skipping entity view. Selected:', selection.toString());
                    return;
                }
                // Get the entity data for this row
                const entity = this.data[idx];
                const entityId = entity.id || entity.entityId;
                if (!entity || !entityId) {
                    console.warn('[QuantumTable] No entity or entityId found for row', idx, entity);
                    return;
                }
                // Parse entityType from ID (e.g., urn:ngsi-ld:Type:ID)
                let entityType = '';
                const urnMatch = entityId.match(/^urn:ngsi-ld:([^:]+):/);
                if (urnMatch) {
                    entityType = urnMatch[1];
                } else if (entity.type || entity.entityType) {
                    entityType = entity.type || entity.entityType;
                }
                // Get date range from localStorage
                let DateTimeFrom = '';
                let DateTimeTo = '';
                try {
                    const state = localStorage.getItem('FormQuantumTableToolbarEditor_state');
                    if (state) {
                        const parsed = JSON.parse(state);
                        DateTimeFrom = parsed.dateRangeFrom || '';
                        DateTimeTo = parsed.dateRangeTo || '';
                    }
                } catch (e) {
                    console.warn('[QuantumTable] Error parsing localStorage date range:', e);
                }
                // Build the object
                // Default DateTimeFrom to start of current month, DateTimeTo to now if not set
                if (!DateTimeFrom) {
                    const now = new Date();
                    DateTimeFrom = new Date(now.getFullYear(), now.getMonth(), 1).toISOString();
                }
                if (!DateTimeTo) {
                    DateTimeTo = new Date().toISOString();
                }
                const entityObj = {
                    EntityID: entityId,
                    entityType: entityType,
                    DateTimeFrom,
                    DateTimeTo
                };
                console.log('[QuantumTable] Double-click: opening entity view with', entityObj);
                // Call the global handler
                if (window.openFormQuantumLeapEntityView) {
                    window.openFormQuantumLeapEntityView(entityObj);
                } else {
                    console.warn('openFormQuantumLeapEntityView is not available');
                }
            });
        });
    }
}

// Add to exports
window.FormQuantumAllTableToolbarEditor = FormQuantumAllTableToolbarEditor;
export default FormQuantumAllTableToolbarEditor;