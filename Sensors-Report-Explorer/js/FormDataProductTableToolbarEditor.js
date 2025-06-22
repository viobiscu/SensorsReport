import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import { DataProductClient } from './api.js';
import { appendToLogs } from './logging.js';

class FormDataProductTableToolbarEditor extends TableToolbarEditorModular {
    constructor(config) {
        console.debug('[FormDataProductTableToolbarEditor] constructor called with config:', config);
        super(config);
        this.dataProductClient = new DataProductClient();
        this.onGet = this.handleGet.bind(this);
        this.onPost = this.handlePost.bind(this);
        this.onPut = this.handlePut.bind(this);
        this.onPatch = this.handlePatch.bind(this);
        this.onDelete = this.handleDelete.bind(this);
        this._render();
    }

    setValue(value) {
        console.debug('[FormDataProductTableToolbarEditor] setValue called with:', value);
        try {
            if (typeof value === 'string') {
                this.data = JSON.parse(value);
            } else if (Array.isArray(value) || (typeof value === 'object' && value !== null)) {
                this.data = value;
            } else {
                throw new Error('setValue expects a JSON string, array, or object');
            }
        } catch (e) {
            console.error('[FormDataProductTableToolbarEditor] setValue error:', e);
            this.data = { error: e.message };
        }
        this.updateDisplay();
        console.debug('[FormDataProductTableToolbarEditor] setValue after update, this.data:', this.data);
    }

    updateDisplay() {
        console.debug('[FormDataProductTableToolbarEditor] updateDisplay called, this.data:', this.data, 'viewMode:', this.viewMode);
        if (typeof this._render === 'function') {
            this._render();
        }
        console.debug('[FormDataProductTableToolbarEditor] updateDisplay after _render');
    }

    async handleGet() {
        let id = this.entityId || (this.data && this.data.id);
        if (!id) return;
        id = id.trim();
        try {
            const dp = await this.dataProductClient.getById(id);
            this.setValue(dp);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
        }
    }
    async handlePost() {
        const data = this.getValue(true);
        try {
            const dp = await this.dataProductClient.create(data);
            // Always show the raw backend response, even for non-JSON or empty responses
            if (typeof dp === 'object') {
                this.setValue(dp);
            } else {
                // Show as string if not JSON
                this.setValue({ raw: dp });
            }
            if (dp && dp.error) {
                appendToLogs('DataProduct POST error: ' + dp.error);
            } else {
                appendToLogs('DataProduct POST success: ' + (typeof dp === 'string' ? dp : JSON.stringify(dp)));
            }
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('DataProduct POST exception: ' + e.message);
        }
    }
    async handlePut() {
        const data = this.getValue(true);
        const id = data && data.id;
        if (!id) return;
        try {
            const dp = await this.dataProductClient.create(data); // DataProductClient may not have update, fallback to create
            this.setValue(dp);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
        }
    }
    async handlePatch() {
        // DataProductClient may not have patch, fallback to create
        await this.handlePut();
    }
    async handleDelete() {
        const id = this.entityId || (this.data && this.data.id);
        if (!id) return;
        try {
            await this.dataProductClient.deleteById(id);
            this.setValue({ message: 'DataProduct deleted', id });
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
        }
    }
    getValue(parseJson = false) {
        if (parseJson) {
            try {
                return JSON.parse(JSON.stringify(this.data));
            } catch (e) {
                return null;
            }
        }
        return this.data;
    }
}

export default FormDataProductTableToolbarEditor;
