import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import { DataProductClient } from './api.js';
import TableRenderer from './TableRenderer.js';
import { appendToLogs } from './logging.js';

class FormDataProductAllTableToolbarEditor extends TableToolbarEditorModular {
    constructor(config) {
        console.debug('[FormDataProductAllTableToolbarEditor] constructor called with config:', config);
        super(config);
        this.dataProductClient = new DataProductClient();
    }

    setValue(value) {
        console.debug('[FormDataProductAllTableToolbarEditor] setValue called with:', value);
        try {
            if (typeof value === 'string') {
                this.data = JSON.parse(value);
            } else if (Array.isArray(value)) {
                this.data = value;
            } else {
                throw new Error('setValue expects a JSON string or array');
            }
        } catch (e) {
            console.error('[FormDataProductAllTableToolbarEditor] setValue error:', e);
            this.data = [];
        }
        this.updateDisplay();
        console.debug('[FormDataProductAllTableToolbarEditor] setValue after update, this.data:', this.data);
    }

    updateDisplay() {
        console.debug('[FormDataProductAllTableToolbarEditor] updateDisplay called, this.data:', this.data, 'viewMode:', this.viewMode);
        if (!Array.isArray(this.data) || this.data.length === 0) {
            if (this.contentEl) this.contentEl.innerHTML = '<div class="empty-table">No data to display</div>';
            return;
        }
        if (!this._tableRenderer) this._tableRenderer = new TableRenderer();
        this._tableRenderer.displayTable(this.data, this.contentEl);
        const table = this.contentEl.querySelector('table.json-table');
        if (table) {
            const columns = Object.keys(this.data[0] || {});
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach((row, idx) => {
                row.addEventListener('dblclick', (event) => this.handleRowDblClick(event, idx, columns));
            });
        }
        console.debug('[FormDataProductAllTableToolbarEditor] updateDisplay after table render');
    }

    async handleRowDblClick(event, idx, columns) {
        const selection = window.getSelection();
        if (selection && selection.toString().length > 0) return;
        const rowData = this.data[idx];
        const dataProductId = rowData.id || rowData.DataProductID;
        if (dataProductId && window.openFormDataProductTableView) {
            window.openFormDataProductTableView(dataProductId);
        } else {
            appendToLogs('openFormDataProductTableView is not available');
        }
    }
}

window.FormDataProductAllTableToolbarEditor = FormDataProductAllTableToolbarEditor;
export default FormDataProductAllTableToolbarEditor;
