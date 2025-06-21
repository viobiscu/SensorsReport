/**
 * JsonAttributeEditor provides a UI for selecting entity attributes
 * This editor displays entity types, entities and their attributes for selection
 */

// Dynamic import to avoid circular dependencies
let OrionLDSearchClient;

/**
 * Class that handles selection of entity attributes
 */
class JsonAttributeEditor {
    constructor() {
        this.selectedType = null;
        this.selectedEntityId = null;
        this.selectedAttribute = null;
        this.callback = null;
        this.modal = null;
        this.entityTypes = [];
        this.entities = [];
        this.attributes = [];
        this.templateAttribute = {
            "attributeName": {
                "type": "Property",
                "value": ""
            }
        };
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
     * @param {Function} callback - Function to call when an attribute is selected
     */
    async showModal(callback) {
        if (typeof callback === 'function') {
            this.callback = callback;
        }

        // Create modal if it doesn't exist
        if (!this.modal) {
            this.modal = this._createModalElement();
            document.body.appendChild(this.modal);
        }

        // Reset the state
        this.selectedType = null;
        this.selectedEntityId = null;
        this.selectedAttribute = null;
        
        // Show the initial options view
        this._showInitialView();
    }

    /**
     * Create the modal element
     * @private
     * @returns {HTMLElement} The modal element
     */
    _createModalElement() {
        const modal = document.createElement('div');
        modal.className = 'attribute-editor-modal';

        const modalContent = document.createElement('div');
        modalContent.className = 'attribute-editor-modal-content';

        // Title
        const title = document.createElement('h2');
        title.textContent = 'Insert Attribute';
        title.className = 'attribute-editor-title';
        modalContent.appendChild(title);

        // Close button
        const closeButton = document.createElement('button');
        closeButton.innerHTML = '✕';
        closeButton.className = 'attribute-editor-close';
        closeButton.onclick = () => this.closeModal();
        modalContent.appendChild(closeButton);

        // Content container
        const contentContainer = document.createElement('div');
        contentContainer.className = 'attribute-editor-content';
        contentContainer.id = 'attributeEditorContent';
        modalContent.appendChild(contentContainer);

        // Add event handlers for clicking outside modal
        modal.addEventListener('click', (e) => {
            if (e.target === modal) {
                this.closeModal();
            }
        });

        modal.appendChild(modalContent);
        return modal;
    }

    /**
     * Show the initial view with options to choose template or copy from existing
     * @private
     */
    _showInitialView() {
        const container = this.modal.querySelector('#attributeEditorContent');
        container.innerHTML = '';

        // Create option cards
        const optionsContainer = document.createElement('div');
        optionsContainer.className = 'attribute-editor-options';

        // Option 1: From Template
        const templateOption = document.createElement('div');
        templateOption.className = 'attribute-option';
        templateOption.innerHTML = `
            <h3 class="attribute-option-title">From Template</h3>
            <p class="attribute-option-desc">Insert a template attribute at the current cursor position.</p>
        `;
        templateOption.onclick = () => this._handleTemplateOption();

        // Option 2: Copy from Existing
        const copyOption = document.createElement('div');
        copyOption.className = 'attribute-option';
        copyOption.innerHTML = `
            <h3 class="attribute-option-title">Copy from Existing</h3>
            <p class="attribute-option-desc">Browse and select attributes from existing entities.</p>
        `;
        copyOption.onclick = () => this._handleCopyOption();

        // Add options to container
        optionsContainer.appendChild(templateOption);
        optionsContainer.appendChild(copyOption);
        container.appendChild(optionsContainer);

        // Show the modal
        this.modal.style.display = 'flex';
    }

    /**
     * Handle the template option selection
     * @private
     */
    _handleTemplateOption() {
        if (this.callback) {
            this.callback(this.templateAttribute);
            this.closeModal();
        }
    }

    /**
     * Handle the copy from existing option
     * @private
     */
    async _handleCopyOption() {
        // Show entity type selection view
        await this._loadEntityTypes();
    }

    /**
     * Load entity types from the API
     * @private
     */
    async _loadEntityTypes() {
        const container = this.modal.querySelector('#attributeEditorContent');
        container.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <div style="display: inline-block; border: 4px solid #f3f3f3; border-top: 4px solid #4CAF50; border-radius: 50%; width: 30px; height: 30px; animation: spin 2s linear infinite;"></div>
                <p style="margin-top: 10px;">Loading entity types...</p>
            </div>
            <style>
                @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                }
            </style>
        `;

        try {
            const client = await this._initClient();
            this.entityTypes = await client.getAllTypes();
            
            // Create entity type selection view with dropdown
            container.innerHTML = `
                <div style="margin-bottom: 20px;">
                    <button id="backToOptionsBtn" style="padding: 6px 12px; border: 1px solid #ddd; background-color: #f5f5f5; border-radius: 4px; cursor: pointer; display: flex; align-items: center;">
                        <span style="margin-right: 5px;">←</span> Back
                    </button>
                </div>
                <div style="margin-bottom: 20px;">
                    <label for="entityTypeSelect" style="display: block; margin-bottom: 5px; font-weight: bold;">Entity Type:</label>
                    <select id="entityTypeSelect" style="width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 4px;">
                        <option value="">Select an entity type</option>
                    </select>
                </div>
            `;
            
            // Back button event
            container.querySelector('#backToOptionsBtn').addEventListener('click', () => this._showInitialView());
            
            // Populate entity types dropdown
            const typesSelect = container.querySelector('#entityTypeSelect');
            
            if (this.entityTypes.length === 0) {
                typesSelect.innerHTML = '<option value="">No entity types found</option>';
            } else {
                this.entityTypes.forEach(type => {
                    const option = document.createElement('option');
                    option.value = type;
                    option.textContent = type;
                    typesSelect.appendChild(option);
                });
                
                // Add change event listener
                typesSelect.addEventListener('change', (e) => {
                    if (e.target.value) {
                        this._onEntityTypeSelected(e.target.value);
                    }
                });
            }
        } catch (error) {
            console.error('Error loading entity types:', error);
            container.innerHTML = `
                <div style="margin-bottom: 20px;">
                    <button id="backToOptionsBtn" style="padding: 6px 12px; border: 1px solid #ddd; background-color: #f5f5f5; border-radius: 4px; cursor: pointer; display: flex; align-items: center;">
                        <span style="margin-right: 5px;">←</span> Back
                    </button>
                </div>
                <div style="padding: 20px; text-align: center; color: #d32f2f;">
                    <p>Error loading entity types: ${error.message}</p>
                </div>
            `;
            
            // Back button event
            container.querySelector('#backToOptionsBtn').addEventListener('click', () => this._showInitialView());
        }
    }

    /**
     * Handle entity type selection
     * @private
     * @param {string} type - The selected entity type
     */
    async _onEntityTypeSelected(type) {
        this.selectedType = type;
        
        // Show loading state
        const container = this.modal.querySelector('#attributeEditorContent');
        container.innerHTML = `
            <div style="text-align: center; padding: 20px;">
                <div style="display: inline-block; border: 4px solid #f3f3f3; border-top: 4px solid #4CAF50; border-radius: 50%; width: 30px; height: 30px; animation: spin 2s linear infinite;"></div>
                <p style="margin-top: 10px;">Loading entities of type "${type}"...</p>
            </div>
        `;
        
        try {
            const client = await this._initClient();
            this.entities = await client.getEntitiesByType(type);
            
            // Create entities view
            this._showEntitiesView();
            
        } catch (error) {
            console.error(`Error loading entities of type ${type}:`, error);
            container.innerHTML = `
                <div style="margin-bottom: 20px;">
                    <button id="backToTypesBtn" style="padding: 6px 12px; border: 1px solid #ddd; background-color: #f5f5f5; border-radius: 4px; cursor: pointer; display: flex; align-items: center;">
                        <span style="margin-right: 5px;">←</span> Back to Types
                    </button>
                </div>
                <div style="padding: 20px; text-align: center; color: #d32f2f;">
                    <p>Error loading entities: ${error.message}</p>
                </div>
            `;
            
            // Back button event
            container.querySelector('#backToTypesBtn').addEventListener('click', () => this._loadEntityTypes());
        }
    }

    /**
     * Show the list of entities for the selected type
     * @private
     */
    _showEntitiesView() {
        const container = this.modal.querySelector('#attributeEditorContent');
        container.innerHTML = `
            <div style="margin-bottom: 20px;">
                <button id="backToTypesBtn" style="padding: 6px 12px; border: 1px solid #ddd; background-color: #f5f5f5; border-radius: 4px; cursor: pointer; display: flex; align-items: center;">
                    <span style="margin-right: 5px;">←</span> Back to Types
                </button>
            </div>
            <h3 style="margin-top: 0; margin-bottom: 15px;">Entities of type "${this.selectedType}"</h3>
            <div id="entitiesList" style="max-height: 400px; overflow-y: auto; border: 1px solid #ddd; border-radius: 8px;"></div>
        `;
        
        // Back button event
        container.querySelector('#backToTypesBtn').addEventListener('click', () => this._loadEntityTypes());
        
        // Populate entities list
        const entitiesList = container.querySelector('#entitiesList');
        
        if (this.entities.length === 0) {
            entitiesList.innerHTML = '<p style="padding: 15px; text-align: center; color: #666;">No entities found for this type.</p>';
        } else {
            this.entities.forEach(entity => {
                const entityItem = document.createElement('div');
                entityItem.className = 'entity-item';
                entityItem.textContent = entity.id || 'Unnamed Entity';
                entityItem.style.padding = '10px 15px';
                entityItem.style.borderBottom = '1px solid #eee';
                entityItem.style.cursor = 'pointer';
                entityItem.style.transition = 'background-color 0.2s';
                
                // Add hover effect
                entityItem.addEventListener('mouseover', () => {
                    entityItem.style.backgroundColor = '#f0f7ff';
                });
                entityItem.addEventListener('mouseout', () => {
                    entityItem.style.backgroundColor = '';
                });
                
                // Add click handler
                entityItem.onclick = () => this._onEntitySelected(entity);
                
                entitiesList.appendChild(entityItem);
            });
        }
    }

    /**
     * Handle entity selection to show its attributes
     * @private
     * @param {Object} entity - The selected entity object
     */
    _onEntitySelected(entity) {
        this.selectedEntityId = entity.id;
        
        // Extract attributes from entity (excluding id, type and @context)
        this.attributes = Object.entries(entity)
            .filter(([key]) => key !== 'id' && key !== 'type' && !key.startsWith('@'))
            .reduce((obj, [key, value]) => {
                obj[key] = value;
                return obj;
            }, {});
        
        // Show attributes view
        this._showAttributesView(entity.id);
    }

    /**
     * Show the attributes of the selected entity
     * @private
     * @param {string} entityId - The ID of the selected entity
     */
    _showAttributesView(entityId) {
        const container = this.modal.querySelector('#attributeEditorContent');
        container.innerHTML = `
            <div style="margin-bottom: 20px;">
                <button id="backToEntitiesBtn" style="padding: 6px 12px; border: 1px solid #ddd; background-color: #f5f5f5; border-radius: 4px; cursor: pointer; display: flex; align-items: center;">
                    <span style="margin-right: 5px;">←</span> Back to Entities
                </button>
            </div>
            <h3 style="margin-top: 0; margin-bottom: 15px;">Attributes of entity "${entityId}"</h3>
            <div id="attributesList" style="max-height: 400px; overflow-y: auto; border: 1px solid #ddd; border-radius: 8px;"></div>
        `;
        
        // Back button event
        container.querySelector('#backToEntitiesBtn').addEventListener('click', () => this._showEntitiesView());
        
        // Populate attributes list
        const attributesList = container.querySelector('#attributesList');
        const attributeKeys = Object.keys(this.attributes);
        
        if (attributeKeys.length === 0) {
            attributesList.innerHTML = '<p style="padding: 15px; text-align: center; color: #666;">No attributes found for this entity.</p>';
        } else {
            attributeKeys.forEach(key => {
                const attrItem = document.createElement('div');
                attrItem.className = 'attribute-item';
                attrItem.textContent = key;
                attrItem.style.padding = '10px 15px';
                attrItem.style.borderBottom = '1px solid #eee';
                attrItem.style.cursor = 'pointer';
                attrItem.style.transition = 'background-color 0.2s';
                
                // Get attribute type for additional display
                const attrValue = this.attributes[key];
                let attrType = 'Unknown';
                if (attrValue && typeof attrValue === 'object' && attrValue.type) {
                    attrType = attrValue.type;
                    
                    // Add type badge
                    const typeBadge = document.createElement('span');
                    typeBadge.textContent = attrType;
                    typeBadge.style.fontSize = '12px';
                    typeBadge.style.fontWeight = 'normal';
                    typeBadge.style.backgroundColor = attrType === 'Property' ? '#e3f2fd' : '#fff8e1';
                    typeBadge.style.color = attrType === 'Property' ? '#1976d2' : '#ff8f00';
                    typeBadge.style.padding = '2px 6px';
                    typeBadge.style.borderRadius = '4px';
                    typeBadge.style.marginLeft = '8px';
                    
                    attrItem.appendChild(typeBadge);
                }
                
                // Add hover effect
                attrItem.addEventListener('mouseover', () => {
                    attrItem.style.backgroundColor = '#f0f7ff';
                });
                attrItem.addEventListener('mouseout', () => {
                    attrItem.style.backgroundColor = '';
                });
                
                // Add click handler
                attrItem.onclick = () => this._onAttributeSelected(key);
                
                attributesList.appendChild(attrItem);
            });
        }
        
        // Add buttons container
        const buttonContainer = document.createElement('div');
        buttonContainer.style.display = 'flex';
        buttonContainer.style.justifyContent = 'flex-end';
        buttonContainer.style.marginTop = '20px';
        buttonContainer.style.gap = '10px';
        
        const cancelButton = document.createElement('button');
        cancelButton.textContent = 'Cancel';
        cancelButton.style.padding = '8px 16px';
        cancelButton.style.border = '1px solid #ddd';
        cancelButton.style.borderRadius = '4px';
        cancelButton.style.backgroundColor = '#f5f5f5';
        cancelButton.style.cursor = 'pointer';
        cancelButton.onclick = () => this.closeModal();
        
        buttonContainer.appendChild(cancelButton);
        container.appendChild(buttonContainer);
    }

    /**
     * Handle attribute selection and return it via callback
     * @private
     * @param {string} attributeName - The name of the selected attribute
     */
    _onAttributeSelected(attributeName) {
        if (this.callback && this.attributes[attributeName]) {
            this.selectedAttribute = attributeName;
            const result = {};
            result[attributeName] = this.attributes[attributeName];
            this.callback(result);
            this.closeModal();
        }
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
window.JsonAttributeEditor = JsonAttributeEditor;
export default JsonAttributeEditor;