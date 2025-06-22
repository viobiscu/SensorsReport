import TableToolbarEditorModular from './TableToolbarEditorModular.js';
import { appendToLogs } from './logging.js';

/**
 * FormEntityTableToolbarEditor - Specialized table editor for displaying entities
 * Extends TableToolbarEditorModular for custom entity table features
 */
class FormEntityTableToolbarEditor extends TableToolbarEditorModular {
    /**
     * @param {Object} config - Configuration options for the table editor
     */
    constructor(config) {
        console.debug('[FormEntityTableToolbarEditor] constructor called with config:', config);
        super(config);
        // Wire NGSI-LD buttons
        this.onInsertAttribute = this.handleInsertAttribute.bind(this);
        this.onInsertRelationship = this.handleInsertRelationship.bind(this);
        this.onInsertContext = this.handleInsertContext.bind(this);
        // Wire API operation buttons
        this.onGet = this.handleGet.bind(this);
        this.onPost = this.handlePost.bind(this);
        this.onPut = this.handlePut.bind(this);
        this.onPatch = this.handlePatch.bind(this);
        this.onDelete = this.handleDelete.bind(this);
        // Re-render toolbar to enable operation buttons after wiring handlers
        this._render();
    }

    setValue(value) {
        console.debug('[FormEntityTableToolbarEditor] setValue called with:', value);
        this.data = value;
        this.updateDisplay();
        console.debug('[FormEntityTableToolbarEditor] setValue after update, this.data:', this.data);
    }

    updateDisplay() {
        console.debug('[FormEntityTableToolbarEditor] updateDisplay called, this.data:', this.data, 'viewMode:', this.viewMode);
        if (typeof this._render === 'function') {
            this._render(); // Force re-render of the table/editor without changing the mode
        }
        console.debug('[FormEntityTableToolbarEditor] updateDisplay after _render');
    }

    // NGSI-LD button handlers
    handleInsertAttribute() {
        // Implementation copied from JsonEditor.insertNGSIAttribute
        import('./jsonAttributeEditor.js').then(({ default: JsonAttributeEditor }) => {
            const attributeEditor = new JsonAttributeEditor();
            attributeEditor.showModal((attribute) => {
                if (typeof this.insertJsonAtCursor === 'function') {
                    this.insertJsonAtCursor(attribute);
                } else {
                    appendToLogs('Attribute inserted: ' + JSON.stringify(attribute));
                }
            });
        });
    }
    handleInsertRelationship() {
        // Implementation copied from JsonEditor.insertNGSIRelationship
        import('./jsonRelationshipEditor.js').then(({ default: JsonRelationshipEditor }) => {
            const relationshipEditor = new JsonRelationshipEditor();
            relationshipEditor.showModal((relationship) => {
                if (typeof this.insertJsonAtCursor === 'function') {
                    this.insertJsonAtCursor(relationship);
                } else {
                    appendToLogs('Relationship inserted: ' + JSON.stringify(relationship));
                }
            });
        });
    }
    handleInsertContext() {
        // Insert @context after the "type" field if in JSON mode and textarea is present
        const contextTemplate = '"@context": [\n  "http://ngsi-ld.sensorsreport.net/synchro-context.jsonld"\n],';
        if (this.mode === 'json' && this.editor && this.editor.textarea) {
            const value = this.editor.textarea.value;
            const typePattern = /"type"\s*:\s*"[^"]*"\s*,/;
            const typeMatch = value.match(typePattern);
            if (typeMatch) {
                const matchPos = typeMatch.index + typeMatch[0].length;
                const beforeMatch = value.substring(0, matchPos);
                const afterMatch = value.substring(matchPos);
                this.editor.textarea.value = beforeMatch + '\n  ' + contextTemplate + afterMatch;
                // Move cursor after inserted context
                const newPosition = matchPos + contextTemplate.length + 3;
                this.editor.textarea.selectionStart = this.editor.textarea.selectionEnd = newPosition;
                this.editor.textarea.focus();
                this.editor.updateDisplay();
                if (typeof this.editor.onChange === 'function') {
                    this.editor.onChange(this.editor.getValue());
                }
                return;
            }
            // fallback: insert at cursor
            const start = this.editor.textarea.selectionStart;
            const end = this.editor.textarea.selectionEnd;
            this.editor.textarea.value = value.substring(0, start) + contextTemplate + value.substring(end);
            this.editor.textarea.selectionStart = this.editor.textarea.selectionEnd = start + contextTemplate.length;
            this.editor.textarea.focus();
            this.editor.updateDisplay();
            if (typeof this.editor.onChange === 'function') {
                this.editor.onChange(this.editor.getValue());
            }
            return;
        }
        // If in table mode, add @context to the data object and refresh
        if (this.mode === 'table' && Array.isArray(this.data)) {
            // Not typical for single entity, but handle gracefully
            this.data.forEach(entity => {
                if (entity && typeof entity === 'object') {
                    entity['@context'] = ["http://ngsi-ld.sensorsreport.net/synchro-context.jsonld"];
                }
            });
            this.updateDisplay();
            return;
        } else if (this.mode === 'table' && this.data && typeof this.data === 'object') {
            this.data['@context'] = ["http://ngsi-ld.sensorsreport.net/synchro-context.jsonld"];
            this.updateDisplay();
            return;
        }
        // Fallback: log
        appendToLogs('Could not insert @context: editor mode or structure not supported.');
    }

    // API operation button handlers
    async handleGet() {
        let entityId = this.entityId || (this.data && this.data.id);
        if (!entityId) {
            appendToLogs('No entity ID specified for GET');
            return;
        }
        entityId = entityId.trim();
        appendToLogs(`Processing GET request for entity ID: ${entityId}`);
        try {
            const OrionLDClient = (await import('./api.js')).OrionLDClient;
            const client = new OrionLDClient();
            const entity = await client.getEntity(entityId);
            if (entity && typeof entity === 'object' && !entity.error) {
                this.setValue(entity);
                appendToLogs(`GET successful for entity ID: ${entityId}`);
            } else {
                this.setValue({
                    error: entity.error || 'Failed to fetch entity',
                    timestamp: new Date().toISOString()
                });
                appendToLogs('GET failed: ' + (entity.error || 'Unknown error'));
            }
        } catch (error) {
            this.setValue({
                error: error.message,
                timestamp: new Date().toISOString()
            });
            appendToLogs('GET failed: ' + error.message);
        }
    }
    async handlePost() {
        // Use OrionLDClient from api.js to create a new entity
        try {
            const OrionLDClient = (await import('./api.js')).OrionLDClient;
            const client = new OrionLDClient();
            const entityData = this.getValue(true);
            if (!entityData) {
                appendToLogs('No entity data to POST');
                return;
            }
            const entity = await client.createEntity(entityData);
            this.setValue(entity);
            appendToLogs(`POST successful for entity ID: ${entity && entity.id ? entity.id : '[unknown]'}`);
        } catch (error) {
            appendToLogs('POST failed: ' + error.message);
        }
    }
    async handlePut() {
        // Use OrionLDClient from api.js to replace an entity
        try {
            const OrionLDClient = (await import('./api.js')).OrionLDClient;
            const client = new OrionLDClient();
            const entityData = this.getValue(true);
            const entityId = entityData && entityData.id;
            if (!entityId) {
                appendToLogs('No entity ID for PUT');
                return;
            }
            const entity = await client.replaceEntity(entityId, entityData);
            this.setValue(entity);
            appendToLogs(`PUT successful for entity ID: ${entityId}`);
        } catch (error) {
            appendToLogs('PUT failed: ' + error.message);
        }
    }
    async handlePatch() {
        // Check if the entity data has context and modify header accordingly

        // First, modify the content-type header if there's a context
        let contentType = 'application/json';
        try {
            const entityData = this.getValue(true);
            // Check if entity has @context property
            if (entityData && (entityData['@context'] || 
                (Array.isArray(entityData) && entityData.some(item => item && item['@context'])))) {
                contentType = 'application/ld+json';
                console.debug('[FormEntityTableToolbarEditor] Found @context, using ld+json content-type');
            }
        } catch (error) {
            console.error('[FormEntityTableToolbarEditor] Error checking for context:', error);
        }
            
            // Set the content type on the client when instantiated later
        // Use OrionLDClient from api.js to update an entity
        try {
            const OrionLDClient = (await import('./api.js')).OrionLDClient;
            const client = new OrionLDClient();
            const entityData = this.getValue(true);
            const entityId = entityData && entityData.id;
            if (!entityId) {
                appendToLogs('No entity ID for PATCH');
                return;
            }
            const entity = await client.updateEntity(entityId, entityData);
            this.setValue(entity);
            appendToLogs(`PATCH successful for entity ID: ${entityId}`);
        } catch (error) {
            appendToLogs('PATCH failed: ' + error.message);
        }
    }
    async handleDelete() {
        // Use OrionLDClient from api.js to delete an entity
        try {
            const OrionLDClient = (await import('./api.js')).OrionLDClient;
            const client = new OrionLDClient();
            const entityId = this.entityId || (this.data && this.data.id);
            if (!entityId) {
                appendToLogs('No entity ID specified for DELETE');
                return;
            }
            await client.deleteEntity(entityId);
            this.setValue({ message: 'Entity deleted', id: entityId });
            appendToLogs(`DELETE successful for entity ID: ${entityId}`);
        } catch (error) {
            appendToLogs('DELETE failed: ' + error.message);
        }
    }
    /**
     * Insert NGSI-LD Context template after the "type" field
     */
    insertNGSIContext = () => {
        const contextTemplate = `"@context": [
    "http://ngsi-ld.sensorsreport.net/synchro-context.jsonld"
    ],`;
    
    const value = this.textarea.value;
    const typePattern = /"type"\s*:\s*"[^"]*"\s*,/;
    const typeMatch = value.match(typePattern);
    
    if (typeMatch) {
        // Found "type": "typename", pattern - insert after it
        const matchPos = typeMatch.index + typeMatch[0].length;
        const beforeMatch = value.substring(0, matchPos);
        const afterMatch = value.substring(matchPos);
        
        // Insert the template after the type field
        this.textarea.value = beforeMatch + '\n  ' + contextTemplate + afterMatch;
        
        // Update cursor position after the inserted template
        const newPosition = matchPos + contextTemplate.length + 3; // +3 for newline and spaces
        this.textarea.selectionStart = this.textarea.selectionEnd = newPosition;
    } else {
        // Fallback: insert at cursor position if "type" field not found
        const start = this.textarea.selectionStart;
        const end = this.textarea.selectionEnd;
        
        // Insert the template at cursor position
        this.textarea.value = this.textarea.value.substring(0, start) 
            + contextTemplate 
            + this.textarea.value.substring(end);
        
        // Update cursor position after the inserted template
        const newPosition = start + contextTemplate.length;
        this.textarea.selectionStart = this.textarea.selectionEnd = newPosition;
    }
    
    // Give focus back to the textarea
    this.textarea.focus();
    
    // Update the display
    this.updateDisplay();
    
    // Show success message
    this.showValidationMessage('NGSI-LD Context template inserted', true);
    
    // Trigger onChange callback
    this.onChange(this.getValue());
}
    // Add custom methods or overrides here as needed

    /**
     * Get the current value from the editor
     * @param {boolean} [parseJson=false] Whether to return a parsed object (deep copy)
     * @returns {Object|string} The current entity data
     */
    getValue(parseJson = false) {
        if (parseJson) {
            try {
                return JSON.parse(JSON.stringify(this.data));
            } catch (e) {
                console.error('[FormEntityTableToolbarEditor] Error parsing data:', e);
                return null;
            }
        }
        return this.data;
    }
}

// Export for global and module usage
window.FormEntityTableToolbarEditor = FormEntityTableToolbarEditor;
export default FormEntityTableToolbarEditor;
