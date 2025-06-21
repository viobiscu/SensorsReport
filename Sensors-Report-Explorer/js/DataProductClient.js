import { appendToLogs } from './logging.js';

class DataProductClient {
    constructor(apiEndpoint) {
        this.apiEndpoint = apiEndpoint;
    }

    async fetchDataProduct(productId) {
        try {
            const response = await fetch(`${this.apiEndpoint}/data-products/${productId}`);
            if (!response.ok) {
                throw new Error(`Error fetching data product: ${response.statusText}`);
            }
            const data = await response.json();
            appendToLogs(`Fetched data product: ${productId}`);
            return data;
        } catch (error) {
            appendToLogs(`Error fetching data product: ${error.message}`);
            throw error;
        }
    }
}

export default DataProductClient;