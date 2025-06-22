class SubscriptionClient {
    constructor() {
        this.baseUrl = '/api/ngsi-ld/v1/subscriptions';
    }

    async getAll() {
        const headers = this._getHeaders();
        const response = await fetch(this.baseUrl, {
            method: 'GET',
            headers
        });
        if (!response.ok) {
            throw new Error(`Failed to fetch subscriptions: ${response.status}`);
        }
        return response.json();
    }

    async getById(id) {
        if (!id) throw new Error('Subscription ID is required');
        const headers = this._getHeaders();
        const response = await fetch(`${this.baseUrl}/${encodeURIComponent(id)}`, {
            method: 'GET',
            headers
        });
        if (!response.ok) {
            throw new Error(`Failed to fetch subscription: ${response.status}`);
        }
        return response.json();
    }

    async create(data) {
        const headers = this._getHeaders(true);
        const response = await fetch(this.baseUrl, {
            method: 'POST',
            headers,
            body: JSON.stringify(data)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to create subscription: ${response.status}, ${errorText}`);
        }
        return response.json();
    }

    async update(id, data) {
        if (!id) throw new Error('Subscription ID is required');
        const headers = this._getHeaders(true);
        const response = await fetch(`${this.baseUrl}/${encodeURIComponent(id)}`, {
            method: 'PATCH',
            headers,
            body: JSON.stringify(data)
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to update subscription: ${response.status}, ${errorText}`);
        }
        return response.json();
    }

    async delete(id) {
        if (!id) throw new Error('Subscription ID is required');
        const headers = this._getHeaders();
        const response = await fetch(`${this.baseUrl}/${encodeURIComponent(id)}`, {
            method: 'DELETE',
            headers
        });
        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`Failed to delete subscription: ${response.status}, ${errorText}`);
        }
        return { success: true };
    }

    _getHeaders(isJson = false) {
        const headers = {
            'Accept': 'application/json'
        };
        if (isJson) headers['Content-Type'] = 'application/json';
        // Add tenant header if present
        const tenant = localStorage.getItem('tenantName');
        if (tenant && tenant.toLowerCase() !== 'default' && tenant !== 'Synchro') {
            headers['NGSILD-Tenant'] = tenant;
        }
        return headers;
    }
}

export default SubscriptionClient;