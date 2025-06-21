/**
 * JsonRelationshipEditor provides a UI for creating entity relationships
 * This editor displays entity types and IDs for creating NGSI-LD relationships
 */

// Dynamic import to avoid circular dependencies
let OrionLDSearchClient;

/**
 * Class that handles selection of entity types and IDs to create relationships
 */
class JsonRelationshipEditor {
    constructor() {
        this.selectedType = null;
        this.selectedEntityId = null;
        this.relationshipType = 'oneToMany'; // Default relationship type
        this.onRelationshipCreated = null; // Callback for when relationship is created
        this.modal = null;
        this.entityTypes = [];
        this.entities = [];
    }

    /**
     * Initialize the API client
     * @private
     */
    async _initClient() {
        if (!OrionLDSearchClient) {
            try {
                const apiModule = await import('./api.js');
                OrionLDSearchClient = apiModule.OrionLDSearchClient;
            } catch (error) {
                console.error('Error loading OrionLDSearchClient:', error);
                throw new Error('Failed to load API client');
            }
        }
        return new OrionLDSearchClient();
    }

    /**
     * Create and show the modal dialog
     * @param {Function} callback - Function to call when a relationship is created
     */
    async showModal(callback) {
        if (typeof callback === 'function') {
            this.onRelationshipCreated = callback;
        }

        // Create modal if it doesn't exist
        if (!this.modal) {
            this.modal = this._createModalElement();
            document.body.appendChild(this.modal);
        }

        // Load entity types
        await this._loadEntityTypes();
    }

    /**
     * Create the modal element
     * @private
     * @returns {HTMLElement} The modal element
     */
    _createModalElement() {
        const modal = document.createElement('div');
        modal.className = 'relationship-editor-modal';

        const modalContent = document.createElement('div');
        modalContent.className = 'relationship-editor-modal-content';

        // Title
        const title = document.createElement('h2');
        title.textContent = 'Create Relationship';
        title.className = 'relationship-editor-title';
        modalContent.appendChild(title);

        // Close button
        const closeButton = document.createElement('button');
        closeButton.innerHTML = '✕';
        closeButton.className = 'relationship-editor-close';
        closeButton.onclick = () => this.closeModal();
        modalContent.appendChild(closeButton);

        // Content container
        const contentContainer = document.createElement('div');
        contentContainer.className = 'relationship-editor-content';
        contentContainer.innerHTML = `
            <div class="relationship-editor-field">
                <label for="entityTypeSelect">Entity Type:</label>
                <select id="entityTypeSelect">
                    <option value="">Loading entity types...</option>
                </select>
            </div>
            <div id="entitiesContainer" class="relationship-editor-field" style="display: none;">
                <label for="entityIdSelect">Entity ID:</label>
                <select id="entityIdSelect">
                    <option value="">Select an entity ID</option>
                </select>
            </div>
            <div class="relationship-editor-field">
                <label>Relationship Type:</label>
                <div class="relationship-editor-radio-group">
                    <div>
                        <input type="radio" id="oneToMany" name="relationshipType" value="oneToMany" checked>
                        <label for="oneToMany">One To Many (EntityTypeBy)</label>
                    </div>
                    <div>
                        <input type="radio" id="manyToOne" name="relationshipType" value="manyToOne">
                        <label for="manyToOne">Many To One (EntityTypeOf)</label>
                    </div>
                </div>
            </div>
        `;
        modalContent.appendChild(contentContainer);

        // Buttons
        const buttonContainer = document.createElement('div');
        buttonContainer.className = 'relationship-editor-button-container';

        const cancelButton = document.createElement('button');
        cancelButton.textContent = 'Cancel';
        cancelButton.className = 'relationship-editor-cancel';
        cancelButton.onclick = () => this.closeModal();

        const createButton = document.createElement('button');
        createButton.textContent = 'Create Relationship';
        createButton.className = 'relationship-editor-create';
        createButton.onclick = () => this._createRelationship();
        createButton.id = 'createRelationshipButton';
        createButton.disabled = true;
        createButton.style.opacity = '0.6';

        buttonContainer.appendChild(cancelButton);
        buttonContainer.appendChild(createButton);
        modalContent.appendChild(buttonContainer);

        modal.appendChild(modalContent);

        // Add event handlers for clicking outside modal
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeModal();
            }
        });

        return modal;
    }

    /**
     * Load entity types from the API
     * @private
     */
    async _loadEntityTypes() {
        try {
            const client = await this._initClient();
            this.entityTypes = await client.getAllTypes();
            
            // Update the select element
            const select = this.modal.querySelector('#entityTypeSelect');
            select.innerHTML = '<option value="">Select an entity type</option>';
            
            this.entityTypes.forEach(type => {
                const option = document.createElement('option');
                option.value = type;
                option.textContent = type;
                select.appendChild(option);
            });

            // Add change event listener
            select.addEventListener('change', (e) => this._onEntityTypeSelected(e.target.value));
            
            // Enable radio button listeners
            const radioButtons = this.modal.querySelectorAll('input[name="relationshipType"]');
            radioButtons.forEach(radio => {
                radio.addEventListener('change', (e) => {
                    this.relationshipType = e.target.value;
                    this._updateCreateButtonState();
                });
            });
        } catch (error) {
            console.error('Error loading entity types:', error);
            this.modal.querySelector('#entityTypeSelect').innerHTML = 
                '<option value="">Error loading entity types</option>';
        }
    }

    /**
     * Handle entity type selection
     * @private
     * @param {string} type - The selected entity type
     */
    async _onEntityTypeSelected(type) {
        this.selectedType = type;
        this._updateCreateButtonState();
        
        const entitiesContainer = this.modal.querySelector('#entitiesContainer');
        const entityIdSelect = this.modal.querySelector('#entityIdSelect');
        
        if (!type) {
            entitiesContainer.style.display = 'none';
            return;
        }

        entitiesContainer.style.display = 'block';
        entityIdSelect.innerHTML = '<option value="">Loading entities...</option>';
        
        try {
            const client = await this._initClient();
            this.entities = await client.getEntitiesByType(type);
            
            entityIdSelect.innerHTML = '<option value="">Select an entity ID</option>';
            
            if (this.entities && this.entities.length > 0) {
                this.entities.forEach(entity => {
                    const option = document.createElement('option');
                    option.value = entity.id;
                    option.textContent = entity.id;
                    entityIdSelect.appendChild(option);
                });
                
                entityIdSelect.addEventListener('change', (e) => {
                    this.selectedEntityId = e.target.value;
                    this._updateCreateButtonState();
                });
            } else {
                entityIdSelect.innerHTML = '<option value="">No entities found</option>';
            }
        } catch (error) {
            console.error(`Error loading entities of type ${type}:`, error);
            entityIdSelect.innerHTML = '<option value="">Error loading entities</option>';
        }
    }

    /**
     * Update the create button state based on selections
     * @private
     */
    _updateCreateButtonState() {
        const createButton = this.modal.querySelector('#createRelationshipButton');
        const isValid = this.selectedType && this.selectedEntityId;
        
        createButton.disabled = !isValid;
        createButton.style.opacity = isValid ? '1' : '0.6';
    }

    /**
     * Create the relationship object and trigger callback
     * @private
     */
    _createRelationship() {
        if (!this.selectedType || !this.selectedEntityId) {
            return;
        }

        // Generate the relationship property name
        const suffix = this.relationshipType === 'oneToMany' ? 'By' : 'Of';
        const relationshipName = `${this.selectedType}${suffix}`;
        
        // Create the relationship object
        const relationship = {
            [relationshipName]: {
                "type": "Relationship",
                "object": this.selectedEntityId
            }
        };

        // Call the callback with the created relationship
        if (typeof this.onRelationshipCreated === 'function') {
            this.onRelationshipCreated(relationship);
        }

        // Close the modal
        this.closeModal();
    }

    /**
     * Close the modal
     */
    closeModal() {
        if (this.modal) {
            this.modal.remove();
            this.modal = null;
        }
    }
}

// Add module exports
window.JsonRelationshipEditor = JsonRelationshipEditor;
export default JsonRelationshipEditor;