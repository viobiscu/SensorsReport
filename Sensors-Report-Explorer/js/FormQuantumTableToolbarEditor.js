import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import { QuantumLeapClient } from './api.js';

/**
 * FormQuantumTableToolbarEditor - Specialized toolbar for time series data operations
 * Extends TableToolbarEditorModular with additional time series specific functionality
 */
class FormQuantumTableToolbarEditor extends TableToolbarEditorModular {
    /**
     * Create a time series toolbar with additional time series specific features
     * @param {Object} config Configuration options
     * @param {string} [config.entityId] ID of the entity for time series data
     * @param {string} [config.entityType] Type of entity for time series data
     * @param {string[]} [config.attributes] Array of attributes to query
     * @param {string} [config.fromDate] Start date for time series query
     * @param {string} [config.toDate] End date for time series query
     * @param {number} [config.lastN] Number of last N values to retrieve
     * @param {string} [config.dateRangeFrom] Start date for date range query
     * @param {string} [config.dateRangeTo] End date for date range query
     */
    constructor(config) {
        // Pass all config to parent, ensure correct instantiation
        super({
            ...config,
            showToolbar: true, // Always show toolbar for time series
            mode: 'table'      // Default to table mode
        });
        // Store time series specific options if needed for later use
        this.entityId = config.entityId || '';
        this.entityType = config.entityType || '';
        this.attributes = config.attributes || [];
        this.fromDate = config.fromDate || '';
        this.toDate = config.toDate || '';
        this.lastN = config.lastN || null;
        this.dateRangeFrom = config.dateRangeFrom || '';
        this.dateRangeTo = config.dateRangeTo || '';

        // Add date range selectors to the toolbar
        this.addDateRangeSelectors();
        this.addGetButtonHandler();
    }

    /**
     * Add two datetime input selectors to the toolbar for dateRangeFrom and dateRangeTo
     */
    addDateRangeSelectors() {
        if (!this.toolbarEl) return;
        // Create from date input
        this.fromInput = document.createElement('input');
        this.fromInput.type = 'datetime-local';
        this.fromInput.value = this.dateRangeFrom;
        this.fromInput.title = 'Start date/time';
        this.fromInput.style.marginLeft = '12px';
        this.fromInput.addEventListener('change', (e) => {
            this.dateRangeFrom = e.target.value;
        });
        // Create to date input
        this.toInput = document.createElement('input');
        this.toInput.type = 'datetime-local';
        this.toInput.value = this.dateRangeTo;
        this.toInput.title = 'End date/time';
        this.toInput.style.marginLeft = '8px';
        this.toInput.addEventListener('change', (e) => {
            this.dateRangeTo = e.target.value;
        });
        // Add labels
        const fromLabel = document.createElement('label');
        fromLabel.textContent = 'From:';
        fromLabel.style.marginLeft = '12px';
        fromLabel.style.fontSize = '13px';
        const toLabel = document.createElement('label');
        toLabel.textContent = 'To:';
        toLabel.style.marginLeft = '8px';
        toLabel.style.fontSize = '13px';
        // Insert into toolbar
        this.toolbarEl.appendChild(fromLabel);
        this.toolbarEl.appendChild(this.fromInput);
        this.toolbarEl.appendChild(toLabel);
        this.toolbarEl.appendChild(this.toInput);
        // After inputs are created, load from localStorage
        this.loadFromLocalStorage();
    }

    /**
     * Attach handler to GET button to fetch time series data from QuantumLeapClient
     */
    addGetButtonHandler() {
        if (!this.toolbarEl) return;
        // Find the GET button in the toolbar
        const getButton = Array.from(this.toolbarEl.querySelectorAll('button')).find(btn => btn.textContent === 'GET');
        if (!getButton) return;
        getButton.addEventListener('click', async () => {
            // Extract entityId from input
            const entityIdInput = this.toolbarEl.querySelector('input[type="text"]');
            const entityId = entityIdInput ? entityIdInput.value.trim() : this.entityId;
            if (!entityId) {
                return;
            }
            // Extract type from entityId (assume urn:ngsi-ld:Type:ID format)
            let entityType = '';
            const urnMatch = entityId.match(/^urn:ngsi-ld:([^:]+):/);
            if (urnMatch) {
                entityType = urnMatch[1];
            }
            // Use date range from selectors
            const fromDate = this.dateRangeFrom;
            const toDate = this.dateRangeTo;
            // Use attributes if set
            const attributes = this.attributes && this.attributes.length > 0 ? this.attributes : undefined;
            // Fetch time series data
            const client = new QuantumLeapClient();
            try {
                const data = await client.getEntityTimeSeries(entityId, entityType, attributes, fromDate, toDate);
                this.setValue(data);
                // Enable search and pagination after data is loaded
                this.showSearch = true;
                this.showPagination = true;
                // Hide entityId input and GET button
                if (entityIdInput) entityIdInput.style.display = 'none';
                getButton.style.display = 'none';
                // Save state to localStorage
                this.saveToLocalStorage(entityId, fromDate, toDate, data);
                // Re-render toolbar to show search and pagination
                this._render();
            } catch (e) {
                console.error('Failed to fetch time series data:', e);
            }
        });
    }

    /**
     * Load persisted state for this editor from localStorage (if available)
     */
    loadFromLocalStorage() {
        try {
            const key = 'FormQuantumTableToolbarEditor_state';
            const saved = localStorage.getItem(key);
            if (saved) {
                const parsed = JSON.parse(saved);
                if (parsed.entityId) {
                    this.entityId = parsed.entityId;
                    // Set entityId input if present
                    const entityIdInput = this.toolbarEl.querySelector('input[type="text"]');
                    if (entityIdInput) entityIdInput.value = parsed.entityId;
                }
                if (parsed.dateRangeFrom) {
                    this.dateRangeFrom = parsed.dateRangeFrom;
                    if (this.fromInput) this.fromInput.value = parsed.dateRangeFrom;
                }
                if (parsed.dateRangeTo) {
                    this.dateRangeTo = parsed.dateRangeTo;
                    if (this.toInput) this.toInput.value = parsed.dateRangeTo;
                }
                // Do NOT auto-populate data. Only restore fields. User must click GET to load data.
            }
        } catch (e) {
            console.error('[Debug] loadFromLocalStorage error:', e);
        }
    }

    /**
     * Save the current state to localStorage
     * @param {string} entityId
     * @param {string} dateRangeFrom
     * @param {string} dateRangeTo
     * @param {Array|Object} data
     */
    saveToLocalStorage(entityId, dateRangeFrom, dateRangeTo, data) {
        try {
            const key = 'FormQuantumTableToolbarEditor_state';
            const toSave = {
                entityId: entityId || this.entityId,
                dateRangeFrom: dateRangeFrom || this.dateRangeFrom,
                dateRangeTo: dateRangeTo || this.dateRangeTo,
                data: data || this.data
            };
            localStorage.setItem(key, JSON.stringify(toSave));
        } catch (e) {
            console.error('[Debug] saveToLocalStorage error:', e);
        }
    }

    /**
     * Set the data for the table and re-render the view
     * @param {string|Array|Object} value - JSON string, array of objects, or object
     */
    setValue(value) {
        try {
            let data;
            if (typeof value === 'string') {
                data = JSON.parse(value);
            } else if (Array.isArray(value)) {
                data = value;
            } else if (typeof value === 'object' && value !== null) {
                data = [value];
            } else {
                throw new Error('setValue expects a JSON string, array, or object');
            }
            // Flatten QuantumLeap time series data if detected
            this.data = this.flattenQuantumLeapData(data);
        } catch (e) {
            console.error('[Debug] setValue error:', e);
            this.data = [];
        }
        this._render();
    }

    /**
     * Transform QuantumLeap time series data to tabular row format for table display
     * @param {Array} data
     * @returns {Array}
     */
    flattenQuantumLeapData(data) {
        if (!Array.isArray(data) || !data[0] || !Array.isArray(data[0].attributes) || !Array.isArray(data[0].index)) {
            return data; // Not QuantumLeap format, return as-is
        }
        const { attributes, index, entityId, entityType } = data[0];
        const attrNames = attributes.map(attr => attr.attrName);
        // Build rows: each row is a timestamp with attribute values
        const rows = index.map((timestamp, i) => {
            const row = {
                timestamp,
                entityId,
                entityType
            };
            attrNames.forEach((attr, j) => {
                row[attr] = attributes[j].values[i];
            });
            return row;
        });
        return rows;
    }

    /**
     * Handle GET request for entity data
     */
    async handleGet() {
        let id = this.entityId || (this.data && this.data.id);
        if (!id) {
            appendToLogs('No entity ID specified for GET');
            return;
        }
        id = id.trim();
        appendToLogs(`[FormQuantumTableToolbarEditor] Processing GET request for entity ID: ${id}`);
        try {
            const QuantumClient = (await import('./api.js')).QuantumClient;
            const client = new QuantumClient();
            const entity = await client.getEntity(id);
            if (entity && typeof entity === 'object' && !entity.error) {
                this.setValue(entity);
                appendToLogs(`GET successful for entity ID: ${id}`);
            } else {
                this.setValue({
                    error: entity.error || 'Failed to fetch entity',
                    timestamp: new Date().toISOString()
                });
                appendToLogs('GET failed: ' + (entity.error || 'Unknown error'));
            }
        } catch (error) {
            this.setValue({
                error: error.message,
                timestamp: new Date().toISOString()
            });
            appendToLogs('GET failed: ' + error.message);
        }
    }
}

window.FormQuantumTableToolbarEditor = FormQuantumTableToolbarEditor;
export default FormQuantumTableToolbarEditor;