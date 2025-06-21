import { appendToLogs } from './logging.js';
import OrionLDClient from './OrionLDClient.js';

class OrionLDSearchClient extends OrionLDClient {
    constructor(baseURL = null, contextURL = null, pageSize = 100) {
        super(baseURL, contextURL);
        this.pageSize = pageSize;
    }

    async getAllEntities(limit = this.pageSize, offset = 0) {
        // Add local=true parameter as the NGSI-LD broker requires at least one filter
        const endpoint = `${this.baseURL}/entities?limit=${limit}&offset=${offset}&local=true`;
        return await this.makeRequest(endpoint, "GET");
    }

    async getEntitiesByType(type, limit = this.pageSize, offset = 0) {
        const endpoint = `${this.baseURL}/entities?type=${encodeURIComponent(type)}&limit=${limit}&offset=${offset}&local=true`;
        return await this.makeRequest(endpoint, "GET");
    }

    // async getSubscriptions() {
    //     const endpoint = `${this.baseURL}/subscriptions`;
    //     return await this.makeRequest(endpoint, "GET");
    // }

    async getAllTypes() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const types = new Set();
            allEntities.forEach(entity => {
                if (entity.type) types.add(entity.type);
            });
            return Array.from(types);
        } catch (error) {
            appendToLogs(`Error fetching entity types: ${error.message}`);
            throw error;
        }
    }

    async getAllAttributes() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const attributes = new Set();
            allEntities.forEach(entity => {
                Object.keys(entity).forEach(key => {
                    if (key !== 'id' && key !== 'type' && !key.startsWith('@')) {
                        attributes.add(key);
                    }
                });
            });
            return Array.from(attributes);
        } catch (error) {
            appendToLogs(`Error fetching attributes: ${error.message}`);
            throw error;
        }
    }

    async getAllRelationships() {
        try {
            const allEntities = await this.fetchAllEntitiesWithPagination();
            const relationships = new Set();
            allEntities.forEach(entity => {
                Object.entries(entity).forEach(([key, value]) => {
                    if (typeof value === 'object' && value !== null &&
                        (value.object || value.type === 'Relationship' ||
                        (Array.isArray(value) && value.some(item => item.object)))) {
                        relationships.add(key);
                    }
                });
            });
            return Array.from(relationships);
        } catch (error) {
            appendToLogs(`Error fetching relationships: ${error.message}`);
            throw error;
        }
    }

    async fetchAllEntitiesWithPagination() {
        const allEntities = [];
        let offset = 0;
        let hasMore = true;

        while (hasMore) {
            const response = await this.getAllEntities(this.pageSize, offset);
            if (Array.isArray(response) && response.length > 0) {
                allEntities.push(...response);
                offset += response.length;
                hasMore = response.length >= this.pageSize;
            } else {
                hasMore = false;
            }
        }
        return allEntities;
    }

    async getAllEntityInformation() {
        try {
            if (store) store.setLoading(true);
            const [types, attributes, subscriptions, relationships] = await Promise.all([
                this.getAllTypes(),
                this.getAllAttributes(),
                this.getSubscriptions(),
                this.getAllRelationships()
            ]);
            return { types, attributes, subscriptions, relationships };
        } catch (error) {
            appendToLogs(`Error fetching entity information: ${error.message}`);
            throw error;
        } finally {
            if (store) store.setLoading(false);
        }
    }
}

export default OrionLDSearchClient;