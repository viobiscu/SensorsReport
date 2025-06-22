import { appendToLogs } from './logging.js';

class QuantumLeapClient {
    constructor(apiKey, baseUrl = '/quantumleap') {
        this.apiKey = apiKey;
        this.baseUrl = baseUrl;
    }

    async fetchData(endpoint) {
        try {
            const response = await fetch(endpoint, {
                headers: {
                    'Authorization': `Bearer ${this.apiKey}`
                }
            });
            const data = await response.json();
            appendToLogs(`Fetched data from ${endpoint}: ${JSON.stringify(data)}`);
            return data;
        } catch (error) {
            appendToLogs(`Error fetching data from ${endpoint}: ${error.message}`);
            throw error;
        }
    }

    async getEntities() {
        const url = `${this.baseUrl}/v2/entities`;
        return this.fetchData(url);
    }

    async getEntityById(entityId, type) {
        if (!entityId) throw new Error('Entity ID is required');
        let url = `${this.baseUrl}/v2/entities/${encodeURIComponent(entityId)}`;
        if (type) url += `?type=${encodeURIComponent(type)}`;
        return this.fetchData(url);
    }

    async getEntityTypes() {
        const url = `${this.baseUrl}/v2/types`;
        return this.fetchData(url);
    }

    async query(params = {}) {
        const url = new URL(`${this.baseUrl}/v2/attrs`);
        Object.entries(params).forEach(([key, value]) => {
            if (value !== undefined && value !== null) {
                url.searchParams.append(key, value);
            }
        });
        return this.fetchData(url.toString());
    }
}

export default QuantumLeapClient;