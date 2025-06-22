from flask import Blueprint, request, jsonify, make_response
import requests
import os
import logging

# Initialize logging
logger = logging.getLogger(__name__)

# Blueprint for NGSI-LD Endpoints
ngsi_ld_blueprint = Blueprint('ngsi_ld', __name__)

class NGSI_LD_Endpoints:
    @staticmethod
    def validate_subscription_payload(payload):
        """Validate the subscription payload according to NGSI-LD specifications"""
        if not payload:
            return False, "Payload cannot be empty"

        # Required fields for a valid subscription
        if not isinstance(payload, dict):
            return False, "Payload must be a JSON object"

        required_fields = ['type', 'notification']

        # Check required fields
        for field in required_fields:
            if field not in payload:
                return False, f"Missing required field: {field}"

        # Validate type
        if payload['type'] != 'Subscription':
            return False, "type field must be 'Subscription'"

        # Validate notification object
        notification = payload.get('notification', {})
        if not isinstance(notification, dict):
            return False, "notification must be an object"

        if 'endpoint' not in notification:
            return False, "notification must contain an endpoint"

        return True, None

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions', methods=['GET'])
    def get_all_subscriptions():
        """Get all subscriptions"""
        try:
            logger.debug("Processing request for get_all_subscriptions endpoint")
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/subscriptions"
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error getting subscriptions: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['GET'])
    def get_subscription(subscription_id):
        """Get a specific subscription by ID"""
        try:
            logger.debug("Processing request for get_subscription endpoint")
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/subscriptions/{subscription_id}"
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error getting subscription {subscription_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions', methods=['POST'])
    def create_subscription():
        """Create a new subscription"""
        try:
            logger.debug("Processing request for create_subscription endpoint")
            payload = request.json
            headers = NGSI_LD_Utils.check_payload(payload, request.headers)
            headers = NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/subscriptions"
            logger.debug(f"Target URL: {target_url}")
            response = requests.post(target_url, headers=headers, json=payload)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error creating subscription: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['PATCH'])
    def update_subscription(subscription_id):
        """Update an existing subscription"""
        try:
            logger.debug("Processing request for update_subscription endpoint")
            payload = request.json
            headers = NGSI_LD_Utils.check_payload(payload, request.headers)
            headers = NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/subscriptions/{subscription_id}"
            logger.debug(f"Target URL: {target_url}")
            response = requests.patch(target_url, headers=headers, json=payload)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error updating subscription {subscription_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['DELETE'])
    def delete_subscription(subscription_id):
        """Delete a subscription"""
        try:
            logger.debug("Processing request for delete_subscription endpoint")
            headers = request.headers
            NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/subscriptions/{subscription_id}"
            logger.debug(f"Target URL: {target_url}")
            response = requests.delete(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error deleting subscription {subscription_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions', methods=['OPTIONS'])
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/subscriptions/<subscription_id>', methods=['OPTIONS'])
    def subscription_options(subscription_id=None):
        """Handle OPTIONS requests for subscriptions endpoints (CORS preflight)"""
        response = make_response()
        response.headers.add('Access-Control-Allow-Origin', '*')
        response.headers.add('Access-Control-Allow-Headers', 'Content-Type, Authorization, NGSILD-Tenant')
        response.headers.add('Access-Control-Allow-Methods', 'GET, POST, PUT, DELETE, PATCH, OPTIONS')
        return response

    # Refactor the `get_entities` method to work correctly with the blueprint
    # Update the `get_entities` function to handle query parameters
    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities', methods=['GET'])
    def get_entities():
        """Fetch a list of entities from the NGSI-LD broker, including query parameters"""
        try:
            logger.debug("Processing request for get_entities endpoint")
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            NGSI_LD_Utils.check_headers(headers)

            # Extract query parameters
            query_params = request.args.to_dict()
            logger.debug(f"Query parameters: {query_params}")
            context_broker_url = os.environ.get('CONTEXT_BROKER_URL')
            target_url = f"{context_broker_url}/ngsi-ld/v1/entities"

            # Send request with query parameters
            logger.debug(f"Sending request to {target_url} with headers and query parameters")
            response = requests.get(target_url, headers=headers, params=query_params)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching entities: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['GET'])
    def get_entity(entity_id):
        """Fetch a specific entity from the NGSI-LD broker"""
        try:
            logger.debug("Processing request for get_entity endpoint")
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/entities/{entity_id}"
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching entity {entity_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities', methods=['POST'])
    def create_entity():
        """Create a new entity in the NGSI-LD broker"""
        try:
            logger.debug("Processing request for create_entity endpoint")
            payload = request.json
            headers = NGSI_LD_Utils.check_payload(payload, request.headers)
            headers = NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/entities"
            logger.debug(f"Target URL: {target_url}")
            response = requests.post(target_url, headers=headers, json=payload)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error creating entity: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['PUT'])
    def replace_entity(entity_id):
        """Replace an entity in the NGSI-LD broker"""
        try:
            logger.debug("Processing request for replace_entity endpoint")
            payload = request.json
            headers = NGSI_LD_Utils.check_payload(payload, request.headers)
            headers = NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/entities/{entity_id}"
            logger.debug(f"Target URL: {target_url}")
            response = requests.put(target_url, headers=headers, json=payload)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error replacing entity {entity_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['PATCH'])
    def update_entity(entity_id):
        """Update an entity in the NGSI-LD broker (partial update)"""
        try:
            logger.debug("Processing request for update_entity endpoint")
            payload = request.json
            headers = NGSI_LD_Utils.check_payload(payload, request.headers)
            headers = NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/entities/{entity_id}"
            logger.debug(f"Target URL: {target_url}")
            response = requests.patch(target_url, headers=headers, json=payload)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error updating entity {entity_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @ngsi_ld_blueprint.route('/api/ngsi-ld/v1/entities/<entity_id>', methods=['DELETE'])
    def delete_entity(entity_id):
        """Delete an entity from the NGSI-LD broker"""
        try:
            logger.debug("Processing request for delete_entity endpoint")
            headers = request.headers
            NGSI_LD_Utils.check_headers(headers)
            target_url = f"{os.environ.get('CONTEXT_BROKER_URL')}/ngsi-ld/v1/entities/{entity_id}"
            logger.debug(f"Target URL: {target_url}")
            response = requests.delete(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error deleting entity {entity_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

class NGSI_LD_Utils:
    @staticmethod
    def check_headers(request_headers):
        """Modify request headers for NGSI-LD requests"""
        logger.debug("Processing check_headers method")
        tenant_id = request_headers.get('NGSILD-Tenant')
        logger.debug(f"Tenant ID: {tenant_id}")
        headers = dict(request_headers)  # Create a mutable copy of headers
        if tenant_id:
            logger.debug(f"Using tenant: {tenant_id}")

            # Remove header if tenant_id is 'Synchro' or 'default'
            if tenant_id.lower() in ['synchro', 'default']:
                logger.debug(f"Removing tenant header for tenant: {tenant_id}")
                headers.pop('NGSILD-Tenant', None)
        return headers

    @staticmethod
    def check_payload(payload, request_headers):
        """Check the payload and set the appropriate Content-Type header"""
        logger.debug("Processing check_payload method")
        headers = dict(request_headers)  # Create a mutable copy of headers
        if payload:
            logger.debug(f"Payload: {payload}")
            if '@context' in payload or "@context" in payload:
                logger.debug("Setting Content-Type to application/ld+json")
                headers['Content-Type'] = 'application/ld+json'
            else:
                logger.debug("Setting Content-Type to application/json")
                headers['Content-Type'] = 'application/json'
        return headers

# Blueprint for Quantum Lead Endpoints
quantum_lead_blueprint = Blueprint('quantum_lead', __name__)

class Quantum_Lead_Endpoints:
    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/version', methods=['GET'])
    def get_version():
        """Fetches the Quantum Lead version"""
        try:
            logger.debug("Processing request for get_version endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/version"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead version: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/health', methods=['GET'])
    def get_health():
        """Fetches the Quantum Lead health status"""
        try:
            logger.debug("Processing request for get_health endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/health"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead health status: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities', methods=['GET'])
    def get_entities():
        """Fetches a list of entities from Quantum Lead"""
        try:
            logger.debug("Processing request for get_entities endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/entities"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead entities: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/types', methods=['GET'])
    def get_types():
        """Fetches entity types from Quantum Lead"""
        try:
            logger.debug("Processing request for get_types endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/types"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead types: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/types/<entity_type>/attrs', methods=['GET'])
    def get_type_attributes(self, entity_type):
        """Fetches attributes for a specific entity type from Quantum Lead"""
        try:
            logger.debug("Processing request for get_type_attributes endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/types/{entity_type}/attrs"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead type attributes: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities/<entity_id>/attrs/<attr_name>', methods=['GET'])
    def get_entity_attribute_values(self, entity_id, attr_name):
        """Fetches time series values for an entity attribute from Quantum Lead"""
        try:
            logger.debug("Processing request for get_entity_attribute_values endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/entities/{entity_id}/attrs/{attr_name}"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead entity attribute values: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities/<entity_id>/attrs/<attr_name>/value', methods=['GET'])
    def get_entity_attribute_last_value(self, entity_id, attr_name):
        """Fetches the last value for an entity attribute from Quantum Lead"""
        try:
            logger.debug("Processing request for get_entity_attribute_last_value endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/entities/{entity_id}/attrs/{attr_name}/value"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead entity attribute last value: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities/<entity_id>/attrs', methods=['GET'])
    def get_entity_attributes(self, entity_id):
        """Fetches all attributes for an entity from Quantum Lead"""
        try:
            logger.debug("Processing request for get_entity_attributes endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/entities/{entity_id}/attrs"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead entity attributes: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities/<entity_id>', methods=['GET'])
    def get_entity_values(self, entity_id):
        """Fetches all values for all attributes of an entity from Quantum Lead"""
        try:
            logger.debug("Processing request for get_entity_values endpoint")
            target_url = f"{os.environ.get('QUANTUM_LEAP_URL')}/v2/entities/{entity_id}"
            headers = request.headers
            logger.debug(f"Request headers: {headers}")
            logger.debug(f"Target URL: {target_url}")
            NGSI_LD_Utils.check_headers(headers)
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching Quantum Lead entity values: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @quantum_lead_blueprint.route('/api/quantum/v2/entities', methods=['OPTIONS'])
    @quantum_lead_blueprint.route('/api/quantum/v2/types', methods=['OPTIONS'])
    @quantum_lead_blueprint.route('/api/quantum/v2/types/<path:subpath>', methods=['OPTIONS'])
    @quantum_lead_blueprint.route('/api/quantum/v2/entities/<path:subpath>', methods=['OPTIONS'])
    def handle_options(self, subpath=None):
        """Handles OPTIONS requests for Quantum Lead endpoints (CORS preflight)"""
        response = make_response()
        response.headers.add('Access-Control-Allow-Origin', '*')
        response.headers.add('Access-Control-Allow-Headers', 'Content-Type, Authorization, Fiware-Service, Fiware-ServicePath')
        response.headers.add('Access-Control-Allow-Methods', 'GET, OPTIONS')
        return response

# Blueprint for Data Product Endpoints
data_product_blueprint = Blueprint('data_product', __name__)

class Data_Product_Endpoints:
    @staticmethod
    @data_product_blueprint.route('/api/dataProducts', methods=['GET'])
    def get_all_data_products():
        """Fetch all data products"""
        try:
            logger.debug("Processing request for get_all_data_products endpoint")
            target_url = f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts"
            headers = {'Accept': 'application/json'}
            logger.debug(f"Target URL: {target_url}")
            logger.debug(f"Request headers: {headers}")
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching all data products: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @data_product_blueprint.route('/api/dataProducts/<data_product_id>', methods=['GET'])
    def get_data_product(data_product_id):
        """Fetch a specific data product by its ID"""
        try:
            logger.debug("Processing request for get_data_product endpoint")
            target_url = f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts/{data_product_id}"
            headers = {'Accept': 'application/json'}
            logger.debug(f"Target URL: {target_url}")
            logger.debug(f"Request headers: {headers}")
            response = requests.get(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error fetching data product {data_product_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @data_product_blueprint.route('/api/dataProducts', methods=['POST'])
    def create_data_product():
        """Create a new data product"""
        try:
            logger.debug("Processing request for create_data_product endpoint")
            if request.content_type and request.content_type.startswith('multipart/form-data'):
                files = {}
                data = {}
                for key in request.form:
                    data[key] = request.form[key]
                for file_key in request.files:
                    files[file_key] = request.files[file_key]
                logger.debug(f"Form data: {data}")
                logger.debug(f"Files: {files}")
                response = requests.post(f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts", data=data, files=files)
            else:
                payload = request.get_json(force=True)
                headers = {'Content-Type': 'application/json', 'Accept': 'application/json'}
                logger.debug(f"Payload: {payload}")
                logger.debug(f"Request headers: {headers}")
                response = requests.post(f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts", json=payload, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error creating data product: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @data_product_blueprint.route('/api/dataProducts', methods=['DELETE'])
    def delete_all_data_products():
        """Delete all data products"""
        try:
            logger.debug("Processing request for delete_all_data_products endpoint")
            target_url = f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts"
            headers = {'Accept': 'application/json'}
            logger.debug(f"Target URL: {target_url}")
            logger.debug(f"Request headers: {headers}")
            response = requests.delete(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error deleting all data products: {str(e)}")
            return jsonify({'error': str(e)}), 500

    @staticmethod
    @data_product_blueprint.route('/api/dataProducts/<data_product_id>', methods=['DELETE'])
    def delete_data_product(data_product_id):
        """Delete a specific data product by its ID"""
        try:
            logger.debug("Processing request for delete_data_product endpoint")
            target_url = f"{os.environ.get('DATA_PRODUCT_URL')}/dataProducts/{data_product_id}"
            headers = {'Accept': 'application/json'}
            logger.debug(f"Target URL: {target_url}")
            logger.debug(f"Request headers: {headers}")
            response = requests.delete(target_url, headers=headers)
            logger.debug(f"Response status code: {response.status_code}")
            return make_response(response.content, response.status_code)
        except Exception as e:
            logger.error(f"Error deleting data product {data_product_id}: {str(e)}")
            return jsonify({'error': str(e)}), 500

blueprints = [ngsi_ld_blueprint, quantum_lead_blueprint, data_product_blueprint]
