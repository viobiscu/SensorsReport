import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import { SubscriptionClient } from './api.js';
import { appendToLogs } from './logging.js';

class FormSubscriptionTableToolbarEditor extends TableToolbarEditorModular {
    constructor(config) {
        console.debug('[FormSubscriptionTableToolbarEditor] constructor called with config:', config);
        super(config);
        this.subscriptionClient = new SubscriptionClient();
        this.onGet = this.handleGet.bind(this);
        this.onPost = this.handlePost.bind(this);
        this.onPut = this.handlePut.bind(this);
        this.onPatch = this.handlePatch.bind(this);
        this.onDelete = this.handleDelete.bind(this);
        this._render();
    }

    setValue(value) {
        console.debug('[FormSubscriptionTableToolbarEditor] setValue called with:', value);
        this.data = value;
        this.updateDisplay();
        console.debug('[FormSubscriptionTableToolbarEditor] setValue after update, this.data:', this.data);
    }

    updateDisplay() {
        console.debug('[FormSubscriptionTableToolbarEditor] updateDisplay called, this.data:', this.data, 'viewMode:', this.viewMode);
        if (typeof this._render === 'function') {
            this._render();
        }
        console.debug('[FormSubscriptionTableToolbarEditor] updateDisplay after _render');
    }

    async handleGet() {
        let id = this.entityId || (this.data && this.data.id);
        if (!id) {
            appendToLogs('No subscription ID specified for GET');
            return;
        }
        id = id.trim();
        appendToLogs(`[FormSubscriptionTableToolbarEditor] Processing GET request for subscription ID: ${id}`);
        try {
            const sub = await this.subscriptionClient.getById(id);
            this.setValue(sub);
            appendToLogs(`GET successful for subscription ID: ${id}`);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('GET failed: ' + e.message);
        }
    }
    async handlePost() {
        const data = this.getValue(true);
        if (!data) {
            appendToLogs('No subscription data to POST');
            return;
        }
        try {
            const sub = await this.subscriptionClient.create(data);
            this.setValue(sub);
            appendToLogs(`POST successful for subscription ID: ${sub && sub.id ? sub.id : '[unknown]'}`);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('POST failed: ' + e.message);
        }
    }
    async handlePut() {
        const data = this.getValue(true);
        const id = data && data.id;
        if (!id) {
            appendToLogs('No subscription ID for PUT');
            return;
        }
        try {
            const sub = await this.subscriptionClient.update(id, data);
            this.setValue(sub);
            appendToLogs(`PUT successful for subscription ID: ${id}`);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('PUT failed: ' + e.message);
        }
    }
    async handlePatch() {
        const data = this.getValue(true);
        const id = data && data.id;
        if (!id) {
            appendToLogs('No subscription ID for PATCH');
            return;
        }
        try {
            const sub = await this.subscriptionClient.update(id, data);
            this.setValue(sub);
            appendToLogs(`PATCH successful for subscription ID: ${id}`);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('PATCH failed: ' + e.message);
        }
    }
    async handleDelete() {
        const id = this.entityId || (this.data && this.data.id);
        if (!id) {
            appendToLogs('No subscription ID specified for DELETE');
            return;
        }
        try {
            await this.subscriptionClient.delete(id);
            this.setValue({ message: 'Subscription deleted', id });
            appendToLogs(`DELETE successful for subscription ID: ${id}`);
        } catch (e) {
            this.setValue({ error: e.message, timestamp: new Date().toISOString() });
            appendToLogs('DELETE failed: ' + e.message);
        }
    }
    getValue(parseJson = false) {
        if (parseJson) {
            try {
                return JSON.parse(JSON.stringify(this.data));
            } catch (e) {
                console.error('[FormSubscriptionTableToolbarEditor] Error parsing data:', e);
                return null;
            }
        }
        return this.data;
    }
}

export default FormSubscriptionTableToolbarEditor;
