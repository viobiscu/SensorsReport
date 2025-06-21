import { appendToLogs } from './logging.js';

// JSON output utility function using window.mainEditor for consistency
function displayJSON(data) {
    if (window.mainEditor) {
        window.mainEditor.setValue(JSON.stringify(data, null, 2));
    } else {
        fallbackDisplayJSON(data);
    }
}

// Fallback display method if main editor is not available
function fallbackDisplayJSON(data) {
    console.log('Fallback JSON display:', data);
}

class OrionLDClient {
    constructor(baseURL) {
        this.baseURL = baseURL;
    }

    async queryEntities(query) {
        try {
            const response = await fetch(`${this.baseURL}/entities?${query}`);
            const data = await response.json();
            appendToLogs(`Query successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Query failed: ${error.message}`);
            throw error;
        }
    }

    async getEntityById(entityId, options = {}) {
        try {
            let url = `${this.baseURL}/entities/${encodeURIComponent(entityId)}`;
            if (options.keyValues) url += '?options=keyValues';
            const response = await fetch(url);
            if (!response.ok) throw new Error(`Failed to get entity: ${response.status}`);
            const data = await response.json();
            appendToLogs(`Get entity successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Get entity failed: ${error.message}`);
            throw error;
        }
    }

    async createEntity(entityData) {
        try {
            const response = await fetch(`${this.baseURL}/entities`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/ld+json' },
                body: JSON.stringify(entityData),
            });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to create entity: ${response.status}, ${errorText}`);
            }
            appendToLogs('Entity created successfully');
            return await response.json().catch(() => ({}));
        } catch (error) {
            appendToLogs(`Create entity failed: ${error.message}`);
            throw error;
        }
    }

    async updateEntity(entityId, updateData) {
        try {
            const response = await fetch(`${this.baseURL}/entities/${entityId}`, {
                method: 'PATCH',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updateData),
            });
            const data = await response.json();
            appendToLogs(`Update successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Update failed: ${error.message}`);
            throw error;
        }
    }

    async deleteEntity(entityId) {
        try {
            const response = await fetch(`${this.baseURL}/entities/${encodeURIComponent(entityId)}`, {
                method: 'DELETE'
            });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to delete entity: ${response.status}, ${errorText}`);
            }
            appendToLogs('Entity deleted successfully');
            return { success: true };
        } catch (error) {
            appendToLogs(`Delete entity failed: ${error.message}`);
            throw error;
        }
    }

    async listEntityTypes() {
        try {
            const response = await fetch(`${this.baseURL}/types`);
            if (!response.ok) throw new Error(`Failed to list types: ${response.status}`);
            const data = await response.json();
            appendToLogs(`List types successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`List types failed: ${error.message}`);
            throw error;
        }
    }

    async getTypeInfo(type) {
        try {
            const response = await fetch(`${this.baseURL}/types/${encodeURIComponent(type)}`);
            if (!response.ok) throw new Error(`Failed to get type info: ${response.status}`);
            const data = await response.json();
            appendToLogs(`Get type info successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Get type info failed: ${error.message}`);
            throw error;
        }
    }

    async getEntityAttributes(entityId) {
        try {
            const response = await fetch(`${this.baseURL}/entities/${encodeURIComponent(entityId)}/attrs`);
            if (!response.ok) throw new Error(`Failed to get entity attributes: ${response.status}`);
            const data = await response.json();
            appendToLogs(`Get entity attributes successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Get entity attributes failed: ${error.message}`);
            throw error;
        }
    }

    async batchUpdate(payload) {
        try {
            const response = await fetch(`${this.baseURL}/entityOperations/upsert`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/ld+json' },
                body: JSON.stringify(payload),
            });
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`Failed to batch update: ${response.status}, ${errorText}`);
            }
            appendToLogs('Batch update successful');
            return await response.json().catch(() => ({}));
        } catch (error) {
            appendToLogs(`Batch update failed: ${error.message}`);
            throw error;
        }
    }

    async getEntityTemporal(entityId, params = {}) {
        try {
            let url = `${this.baseURL}/temporal/entities/${encodeURIComponent(entityId)}`;
            const search = new URLSearchParams(params).toString();
            if (search) url += `?${search}`;
            const response = await fetch(url);
            if (!response.ok) throw new Error(`Failed to get temporal entity: ${response.status}`);
            const data = await response.json();
            appendToLogs(`Get temporal entity successful: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Get temporal entity failed: ${error.message}`);
            throw error;
        }
    }
}

export default OrionLDClient;
export { displayJSON, fallbackDisplayJSON };