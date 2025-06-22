import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import TableRenderer from './TableRenderer.js';

class FormEntityAllByTypeTableToolbarEditor extends TableToolbarEditorModular {
    constructor(config) {
        super(config);
    }

    setValue(value) {
        try {
            if (typeof value === 'string') {
                this.data = JSON.parse(value);
            } else if (Array.isArray(value)) {
                this.data = value;
            } else {
                throw new Error('setValue expects a JSON string or array');
            }
        } catch (e) {
            console.error('[FormEntityAllByTypeTableToolbarEditor] setValue error:', e);
            this.data = [];
        }
        this.updateDisplay();
    }

    updateDisplay() {
        if (!Array.isArray(this.data) || this.data.length === 0) {
            if (this.contentEl) this.contentEl.innerHTML = '<div class="empty-table">No data to display</div>';
            return;
        }
        if (!this._tableRenderer) this._tableRenderer = new TableRenderer();
        this._tableRenderer.displayTable(this.data, this.contentEl);
        const table = this.contentEl.querySelector('table.json-table');
        if (table) {
            const isTypeCount = this.data[0] && Object.keys(this.data[0]).length === 2 && 'type' in this.data[0] && 'count' in this.data[0];
            const columns = isTypeCount ? ['type', 'count'] : Object.keys(this.data[0] || {});
            const rows = table.querySelectorAll('tbody tr');
            rows.forEach((row, idx) => {
                row.addEventListener('dblclick', (event) => this.handleRowDblClick(event, idx, isTypeCount, columns));
            });
        }
    }

    async handleRowDblClick(event, idx, isTypeCount, columns) {
        const selection = window.getSelection();
        if (selection && selection.toString().length > 0) return;
        const rowData = this.data[idx];
        const entityType = rowData.type;
        const entityId = rowData.id || rowData.EntityID;
        if (isTypeCount && entityType && !entityId) {
            if (window.openFormEntityAllOfTypeTableView) {
                window.openFormEntityAllOfTypeTableView(entityType);
            } else {
                alert('openFormEntityAllOfTypeTableView is not available');
            }
            return;
        }
        if (!isTypeCount && entityId) {
            if (window.openFormEntityTableView) {
                window.openFormEntityTableView(entityId);
            } else {
                alert('openFormEntityTableView is not available');
            }
        }
    }
}

window.FormEntityAllByTypeTableToolbarEditor = FormEntityAllByTypeTableToolbarEditor;
export default FormEntityAllByTypeTableToolbarEditor;
