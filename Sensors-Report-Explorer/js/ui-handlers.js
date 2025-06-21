/**
 * UI handler utilities for the application
 * Complements the main UI manager with additional handlers
 */
import { TabManager } from './TabManager.js';
import { appendToLogs } from './logging.js';
import { clearLogs } from './logging.js';
import { OrionLDClient, OrionLDSearchClient, QuantumLeapClient } from './api.js';
import FormQuantumAllTableToolbarEditor from './FormQuantumAllTableToolbarEditor.js';
import FormQuantumTableToolbarEditor from './FormQuantumTableToolbarEditor.js';
import FormEntityAllByTypeTableToolbarEditor from './FormEntityAllByTypeTableToolbarEditor.js';
import FormEntityTableToolbarEditor from './FormEntityTableToolbarEditor.js';

// 
// /**
//  * Get a template for entity operations based on mode
//  * @param {string} mode - The operation mode (get, post, put, patch)
//  * @returns {Object} A template object for the specified mode
//  */
// function __DELETE_getEntityTemplate(mode) {
//     switch (mode.toLowerCase()) {
//         case 'post':
//         case 'put':
//             return {
//                 "id": "urn:ngsi-ld:Entity:001",
//                 "type": "Entity",
//                 "@context": ["https://ngsi-ld.sensorsreport.net/synchro-context.jsonld"]
//             };
//         case 'patch':
//             return {
//                 "@context": ["https://ngsi-ld.sensorsreport.net/synchro-context.jsonld"],
//                 "value": "new value"
//             };
//         case 'get':
//         default:
//             return {
//                 message: "Enter an Entity ID and click GET",
//                 instructions: "Example ID: urn:ngsi-ld:Entity:001"
//             };
//     }
// }

/**
 * Toggle a tree view element's expanded state
 * @param {HTMLElement} element - The tree view element to toggle
 */
export function toggleTreeView(element) {
    if (!element?.classList?.contains('expandable')) return;

    element.classList.toggle('active');
    const nestedList = element.nextElementSibling;
    
    if (nestedList && nestedList.classList.contains('nested')) {
        nestedList.style.display = 
            nestedList.style.display === 'block' ? 'none' : 'block';
    }
}

/**
 * Initialize all tree view elements in the document
 */
export function initTreeView() {
    document.querySelectorAll('.treeview-item').forEach(item => {
        item.addEventListener('click', (e) => toggleTreeView(e.currentTarget));
    });

    // Prevent event bubbling for nested items
    document.querySelectorAll('.nested li').forEach(item => {
        item.addEventListener('click', (e) => e.stopPropagation());
    });
}

/**
 * Initialize all UI elements and event listeners
 */
export function initializeUI() {
    document.addEventListener('DOMContentLoaded', () => {
        setupEventListeners();
        appendToLogs('UI initialized successfully');
    });
}

/**
 * Set up event listeners for UI elements
 */
function setupEventListeners() {
    // Set up tenant selection input
    const tenantSelect = document.getElementById('tenantname');
    if (tenantSelect) {
        tenantSelect.addEventListener('change', () => {
            localStorage.setItem('tenantName', tenantSelect.value);
            appendToLogs(`Tenant changed to: ${tenantSelect.value}`);
        });
    }

    const btnListByType = document.getElementById('btn-list-by-type-get');
    console.log('[ListByType] DOMContentLoaded, btnListByType:', btnListByType);
    if (btnListByType) {
        console.log('[ListByType] Attaching click handler to List By Type Get button');
        btnListByType.addEventListener('click', async () => {
            console.log('[ListByType] List By Type Get clicked');
            // Fetch all entities grouped by type
            const client = new OrionLDSearchClient();
            let typeRows = [];
            try {
                // Fetch all types
                const types = await client.getAllTypes();
                // For each type, fetch entities and count them
                for (const type of types) {
                    const entities = await client.getEntitiesByType(type);
                    typeRows.push({
                        type: type,
                        count: Array.isArray(entities) ? entities.length : 0
                    });
                }
            } catch (e) {
                alert('Failed to fetch entities by type: ' + e);
                return;
            }
            // Prepare options for the editor
            const editorOptions = {
                mode: 'table',
                showSearch: true,
                showPagination: true,
                showColumnToggle: true,
                data: typeRows,
                editorClass: FormEntityAllByTypeTableToolbarEditor
            };
            // Initialize TabManager if needed
            if (!window.tabManager) {
                const { TabManager } = await import('./TabManager.js');
                window.tabManager = await TabManager.initialize();
            }
            // Create a new tab
            const tabTitle = 'Entities By Type';
            window.tabManager.createEditorTab(tabTitle, editorOptions);
        });
    } else {
        console.warn('[ListByType] btn-list-by-type-get not found in DOM');
    }
}

// /**
//  * Opens the entity editor in the specified mode
//  * @param {string} mode - The mode to open the editor in (get, post, put, patch)
//  */
// export function __DELETE_openEntityEditor(mode) {
//     console.log(`Opening JSON editor directly in ${mode} mode`);
    
//     // Initialize TabManager if it doesn't exist yet
//     if (!window.tabManager) {
//         console.log('Initializing TabManager');
//         window.tabManager = new TabManager('#displayArea');
//     }
    
//     // Initial entity templates based on mode
//     const entityTemplate = getEntityTemplate(mode);
    
//     // Get saved values if available
//     let entityId = '';
//     let initialValue = null;
    
//     // Try to load saved values for the current mode
//     const savedJson = localStorage.getItem(`entity${mode.charAt(0).toUpperCase() + mode.slice(1)}Json`);
//     const savedEntityId = localStorage.getItem(`entity${mode.charAt(0).toUpperCase() + mode.slice(1)}Id`);
    
//     if (savedJson) {
//         try {
//             initialValue = JSON.parse(savedJson);
//         } catch (e) {
//             console.warn('Could not parse saved JSON, using template instead', e);
//             initialValue = entityTemplate;
//         }
//     } else {
//         initialValue = entityTemplate;
//     }
    
//     // Use saved entity ID for all suitable modes
//     if (savedEntityId && ['get', 'put', 'patch', 'delete'].includes(mode)) {
//         entityId = savedEntityId;
//     } else if (['put', 'patch'].includes(mode)) {
//         // For PUT/PATCH, extract ID from template if no saved ID
//         entityId = initialValue.id || '';
//     }
    
//     // Create tab title based on the mode
//     const tabTitle = `${mode.toUpperCase()} Entity`;
    
//     // Create editor options
//     const editorOptions = {
//         initialValue: JSON.stringify(initialValue, null, 2),
//         height: 500,
//         resize: true,
//         mode: mode,
//         entityId: entityId,
//         operation: mode.toUpperCase(), // Explicitly set the operation type to trigger API buttons
//         allowEntityIdEdit: true,       // Allow editing the entity ID
//         onApiOperation: function(operation, entityId, data) {
//             console.log(`API operation called: ${operation} on entity ${entityId}`);
            
//             // Handler for different operations
//             if (operation === 'GET') {
//                 if (typeof window.handleGetQuery === 'function') {
//                     console.log(`Handling GET for ${entityId}`);
//                     window.handleGetQuery(entityId);
//                     return { success: true };
//                 }
//             } else if (operation === 'POST') {
//                 if (typeof window.processPostQuery === 'function') {
//                     return window.processPostQuery(JSON.stringify(data));
//                 }
//             } else if (operation === 'PUT') {
//                 if (typeof window.processPutQuery === 'function') {
//                     return window.processPutQuery(entityId, JSON.stringify(data));
//                 }
//             } else if (operation === 'PATCH') {
//                 if (typeof window.processPatchQuery === 'function') {
//                     return window.processPatchQuery(entityId, JSON.stringify(data));
//                 }
//             } else if (operation === 'DELETE') {
//                 if (typeof window.processDeleteQuery === 'function') {
//                     return window.processDeleteQuery(entityId);
//                 }
//             }
            
//             console.error(`No handler for ${operation} operation`);
//             return { error: true, message: `Handler for ${operation} not available` };
//         },
//         onSave: function(value) {
//             try {
//                 const parsedValue = JSON.parse(value);
//                 localStorage.setItem(`entity${mode.charAt(0).toUpperCase() + mode.slice(1)}Json`, JSON.stringify(parsedValue));
//                 console.log(`Saved ${mode} JSON to localStorage`);
//             } catch (e) {
//                 console.error('Error saving JSON to localStorage:', e);
//             }
//         }
//     };
    
//     // Create a new tab with the editor
//     const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
//     console.log(`Created new tab with ID: ${tabId} for ${mode} mode`);
    
//     // For backward compatibility, set window.entityEditor to the new editor
//     const tab = window.tabManager.getTabById(tabId);
//     if (tab && tab.editor) {
//         window.entityEditor = tab.editor;
        
//         // For GET mode, also set the getResultsEditor reference
//         if (mode === 'get') {
//             window.getResultsEditor = tab.editor;
//         }
//     }
    
//     // Log the operation
//     if (typeof appendToLogs === 'function') {
//         appendToLogs(`Opened ${mode.toUpperCase()} Entity editor in a new tab`);
//     }
// }

/**
 * Opens a menu item in a new tab
 * @param {string} path - The path to the content HTML file
 * @param {string} title - The title for the tab
 */
export async function openMenuItemInTab(path, title) {
    console.log(`Opening ${title} in new tab`);
    
    // Initialize TabManager if it doesn't exist yet
    if (!window.tabManager) {
        console.log('Initializing TabManager');
        window.tabManager = new TabManager('#displayArea');
    }
    
    // Store current content for returning from color settings
    if (title === 'Color Settings') {
        const currentContent = document.getElementById('displayArea').innerHTML;
        localStorage.setItem('previousContent', currentContent);
    }
    
    try {
        const response = await fetch(path);
        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
        const content = await response.text();
        
        // Create a unique tab ID
        const tabId = `tab-${Date.now()}`;
        
        // Create the tab element
        const tab = window.tabManager.createTab(title, tabId);
        
        // Create content container
        const contentContainer = document.createElement('div');
        contentContainer.className = 'editor-tab-content';
        contentContainer.id = `${tabId}-content`;
        contentContainer.style.height = '100%';
        contentContainer.style.display = 'none';
        contentContainer.style.padding = '20px';
        contentContainer.style.overflow = 'auto';
        
        // Set the HTML content directly
        contentContainer.innerHTML = content;
        
        // Add to tabs array and DOM
        window.tabManager.contentArea.appendChild(contentContainer);
        window.tabManager.tabs.push({
            id: tabId,
            tabElement: tab,
            contentElement: contentContainer,
            mode: 'html'
        });
        
        // Activate the new tab
        window.tabManager.activateTab(tabId);
        
        // Initialize color settings if needed
        if (title === 'Color Settings' && typeof window.initColorSettings === 'function') {
            setTimeout(() => window.initColorSettings(), 100);
        }
        
        appendToLogs(`Opened ${title} in a new tab`);
    } catch (error) {
        console.error('Error opening menu item:', error);
        appendToLogs(`Error opening ${title}: ${error.message}`);
    }
}

// /**
//  * Handle entity GET operation
//  * @param {string} path - The path to get content from
//  */
// export async function __DELETE_handleEntityGet(path) {
//     console.log('Handling GET for path:', path);
    
//     // Extract the title from the path
//     const title = path.split('/').pop().replace('.html', '');
//     const formattedTitle = title.split(/(?=[A-Z])/).join(' ');
    
//     // Special handling for Queue, Resources and Color Settings
//     if (path.includes('queue.html') || path.includes('resources.html') || path.includes('colorSettings.html')) {
//         await openMenuItemInTab(path, formattedTitle);
//         return;
//     }
    
//     // Original behavior for other items
//     try {
//         const response = await fetch(path);
//         if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
//         const content = await response.text();
        
//         // Update the display area
//         const displayArea = document.getElementById('displayArea');
//         if (displayArea) {
//             displayArea.innerHTML = content;
//         }
        
//         appendToLogs(`Loaded ${formattedTitle} view`);
//     } catch (error) {
//         console.error('Error loading content:', error);
//         appendToLogs(`Error loading ${formattedTitle}: ${error.message}`);
//     }
// }

// /**
//  * Process PATCH request for entity updates
//  * @param {string} entityId - The ID of the entity to update
//  * @param {string} jsonData - The JSON data containing the updates
//  * @returns {Promise} Promise that resolves with the patch result
//  */
// export async function __DELETE_processPatchQuery(entityId, jsonData) {
//     try {
//         const client = new OrionLDClient();
//         const entityData = JSON.parse(jsonData);
        
//         // Call updateEntity on the client
//         const result = await client.updateEntity(entityId, entityData);
//         appendToLogs(`Successfully processed PATCH request for ${entityId}`);
//         return result;
//     } catch (error) {
//         console.error('Error processing PATCH:', error);
//         appendToLogs(`Error processing PATCH: ${error.message}`);
//         throw error;
//     }
// }

// /**
//  * Process PUT request for entity replacement
//  * @param {string} entityId - The ID of the entity to replace
//  * @param {string} jsonData - The JSON data containing the full entity
//  * @returns {Promise} Promise that resolves with the put result
//  */
// export async function __DELETE_processPutQuery(entityId, jsonData) {
//     try {
//         const client = new OrionLDClient();
//         const entityData = JSON.parse(jsonData);
        
//         // Call replaceEntity on the client
//         const result = await client.replaceEntity(entityId, entityData);
//         appendToLogs(`Successfully processed PUT request for ${entityId}`);
//         return result;
//     } catch (error) {
//         console.error('Error processing PUT:', error);
//         appendToLogs(`Error processing PUT: ${error.message}`);
//         throw error;
//     }
// }

// /**
//  * Process DELETE request for entity deletion
//  * @param {string} entityId - The ID of the entity to delete
//  * @returns {Promise} Promise that resolves with the delete result
//  */
// export async function __DELETE_processDeleteQuery(entityId) {
//     try {
//         const client = new OrionLDClient();
        
//         // Call deleteEntity on the client
//         const result = await client.deleteEntity(entityId);
//         appendToLogs(`Successfully processed DELETE request for ${entityId}`);
//         return result;
//     } catch (error) {
//         console.error('Error processing DELETE:', error);
//         appendToLogs(`Error processing DELETE: ${error.message}`);
//         throw error;
//     }
// }

// /**
//  * Process DELETE request for subscription deletion
//  * @param {string} subscriptionId - The ID of the subscription to delete
//  * @returns {Promise} Promise that resolves with the delete result
//  */
// export async function __DELETE_processDeleteSubscriptionData() {
//     try {
//         let subscriptionId = prompt('Enter the Subscription ID to delete:');
//         if (!subscriptionId) return;

//         // Add confirmation prompt
//         const confirmed = confirm(`Are you sure you want to delete subscription with ID: ${subscriptionId}?`);
//         if (!confirmed) {
//             appendToLogs(`Delete operation canceled for subscription: ${subscriptionId}`);
//             return;
//         }

//         const response = await fetch(`/api/ngsi-ld/v1/subscriptions/${subscriptionId}`, {
//             method: 'DELETE',
//             headers: {
//                 'Accept': 'application/json'
//             }
//         });

//         if (!response.ok) {
//             const errorText = await response.text();
//             throw new Error(`HTTP error! status: ${response.status}, ${errorText}`);
//         }

//         appendToLogs(`Successfully deleted subscription: ${subscriptionId}`);
//         return { success: true };
//     } catch (error) {
//         console.error('Error deleting subscription:', error);
//         appendToLogs(`Error deleting subscription: ${error.message}`);
//         throw error;
//     }
// }

/**
 * Opens a table view of all subscriptions in a new tab
 */
// export function __DELETE_openSubscriptionTableView() {
//     // Initialize TabManager if it doesn't exist yet
//     if (!window.tabManager) {
//         console.log('Initializing TabManager');
//         window.tabManager = new TabManager('#displayArea');
//     }

//     // Create tab title
//     const tabTitle = 'Subscriptions';

//     // First fetch the subscriptions
//     const headers = {
//         'Accept': 'application/json',
//         'Cache-Control': 'no-cache'
//     };
    
//     // Get tenant ID if available
//     const tenant = localStorage.getItem('tenantName');
//     if (tenant && tenant.toLowerCase() !== 'default' && tenant !== 'Synchro') {
//         headers['NGSILD-Tenant'] = tenant;
//     }

//     // Create editor options with initial empty array
//     const editorOptions = {
//         height: 500,
//         resize: true,
//         mode: 'table',
//         showToolbar: true,
//         initialValue: '[]',
//         editorClass: JsonTableEditor
//     };

//     // Fetch subscriptions first, then update the editor
//     fetch('/api/ngsi-ld/v1/subscriptions', {
//         method: 'GET',
//         headers: headers
//     })
//     .then(async response => {
//         if (!response.ok) {
//             const errorText = await response.text();
//             throw new Error(`HTTP error! status: ${response.status}, body: ${errorText}`);
//         }
//         return response.json();
//     })
//     .then(subscriptions => {
//         console.log('[Debug] Received subscriptions:', {
//             count: subscriptions?.length,
//             data: subscriptions
//         });
        
//         appendToLogs(`Successfully fetched ${subscriptions?.length || 0} subscriptions`);

//         // Update editor options with fetched data
//         if (Array.isArray(subscriptions)) {
//             editorOptions.initialValue = JSON.stringify(subscriptions, null, 2);
//         }
        
//         // Create a new tab with the table editor
//         const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
//         console.log('[Debug] Created subscription table tab:', { tabId });

//         // Log the operation
//         appendToLogs('Opened Subscriptions table view in a new tab');
//     })
//     .catch(error => {
//         console.error('[Debug] Error fetching subscriptions:', error);
//         appendToLogs(`Error fetching subscriptions: ${error.message}`);
        
//         // Create tab with empty array in case of error
//         const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
//         console.log('[Debug] Created empty subscription table tab due to error:', { tabId });
//     });
// }

// /**
//  * Opens a subscription editor in a new tab
//  * @param {string} mode - The mode for the editor (get, post, etc.)
//  */
// export function __DELETE_openSubscriptionEditor(mode) {
//     // Initialize TabManager if it doesn't exist yet
//     if (!window.tabManager) {
//         console.log('Initializing TabManager');
//         window.tabManager = new TabManager('#displayArea');
//     }

//     const tabTitle = 'Subscription Editor';
//     const editorOptions = {
//         height: '100%',
//         resize: true,
//         showLineNumbers: false,
//         mode: mode,
//         editorClass: JsonEditorSubscription
//     };

//     // Create a new tab with the subscription editor
//     const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
//     console.log(`Created new subscription editor tab with ID: ${tabId}`);

//     if (typeof appendToLogs === 'function') {
//         appendToLogs('Opened subscription editor');
//     }
// }

/**
 * Validates the TableToolbarEditor class
 */
// const __DELETE_validateTableToolbarEditor = () => {
//     console.log('[Debug] TableToolbarEditor validation:', {
//         isDefined: typeof TableToolbarEditor !== 'undefined',
//         isClass: TableToolbarEditor?.prototype?.constructor?.name,
//         hasProto: !!TableToolbarEditor?.prototype,
//         methods: Object.getOwnPropertyNames(TableToolbarEditor?.prototype || {})
//     });
// };

/**
 * Opens a table view of all Quantum Leap entities in a new tab
 */
export function openFormQuantumLeapTableView() {
    // Initialize TabManager if it doesn't exist yet
    if (!window.tabManager) {
        console.log('Initializing TabManager');
        window.tabManager = new TabManager('#displayArea');
    }

    // Create tab title
    const tabTitle = 'QuantumLeap Table';

    // Create editor options
    const editorOptions = {
        height: 500,
        resize: true,
        mode: 'table',
        showToolbar: true,
        initialValue: '[]',
        editorClass: FormQuantumAllTableToolbarEditor,
        // Configure visible toolbar buttons and other table options
        visibleToolbarButtons: {
            separator: false,
            attribute: false,
            relationship: false,
            context: false,
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true
        },
        // Enable GET operation
        operation: '',
        allowEntityIdEdit: true,
        showSearch: false,
        showPagination: true,
        showColumnToggle: true,
        pageSize: 10
    };

    console.log('[Debug] Creating QuantumLeap table view with options:', {
        mode: editorOptions.mode,
        editorClass: editorOptions.editorClass.name,
        showToolbar: editorOptions.showToolbar,
        visibleToolbarButtons: editorOptions.visibleToolbarButtons,
        quantumMode: editorOptions.quantumMode
    });

    // Create a new QuantumLeapClient instance
    const client = new QuantumLeapClient();

    // Fetch entities without specifying a type
    client.getEntitiesTimeSeries()
        .then(entities => {
            console.log('[Debug] Received Quantum Leap entities:', {
                type: typeof entities,
                isArray: Array.isArray(entities),
                length: Array.isArray(entities) ? entities.length : 'N/A',
                firstEntity: Array.isArray(entities) && entities.length > 0 ? entities[0] : null
            });
            // Update editor options with fetched data
            if (Array.isArray(entities)) {
                editorOptions.initialValue = JSON.stringify(entities, null, 2);
                appendToLogs(`Successfully fetched ${entities.length} entities from Quantum Leap`);
            } else {
                appendToLogs(`Successfully fetched entity data from Quantum Leap (not an array)`);
            }

            // Create a new tab with the table editor
            const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
            console.log(`Created QuantumLeap table tab with ID: ${tabId}`);

            // Log the operation
            appendToLogs('Opened QuantumLeap table view in a new tab');
            appendToLogs(`Successfully fetched entities from Quantum Leap`);

            // Update editor with fetched data
            const tab = window.tabManager.getTabById(tabId);
            console.log('[Debug] Tab after fetch:', tab);
            if (tab && tab.editor) {
                console.log('[Debug] Tab editor instance:', tab.editor);
                if (typeof tab.editor.setValue === 'function') {
                    console.log('[Debug] Calling setValue on editor with entities:', entities);
                    tab.editor.setValue(JSON.stringify(entities, null, 2));
                } else {
                    console.warn('[Debug] tab.editor.setValue is not a function');
                }
                if (typeof tab.editor.updateDisplay === 'function') {
                    console.log('[Debug] Calling updateDisplay on editor');
                    tab.editor.updateDisplay();
                } else {
                    console.warn('[Debug] tab.editor.updateDisplay is not a function');
                }
                if (typeof tab.editor.getValue === 'function') {
                    const currentValue = tab.editor.getValue(true);
                    console.log('[Debug] Editor getValue(true) after setValue:', currentValue);
                }
            } else {
                console.warn('[Debug] Tab or tab.editor not found after fetch');
            }
        })
        .catch(error => {
            console.error('[Debug] Error fetching Quantum Leap entities:', error);
        });

    // Create a new tab with the table editor
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    console.log(`Created QuantumLeap table tab with ID: ${tabId}`);

    if (typeof appendToLogs === 'function') {
        appendToLogs('Opened QuantumLeap table view in a new tab');
    }
}

/**
 * Opens an entity view for Quantum Leap data
 */
export function openFormQuantumLeapEntityView(entityObj) {
    // Initialize TabManager if it doesn't exist yet
    if (!window.tabManager) {
        console.log('Initializing TabManager');
        window.tabManager = new TabManager('#displayArea');
    }

    // Create tab title
    const tabTitle = `Quantum Leap Entity View: ${entityObj.EntityID}`;

    // Create editor options
    const editorOptions = {
        height: 500,
        resize: true,
        mode: 'table',
        showToolbar: true,
        initialValue: '[]',
        editorClass: FormQuantumTableToolbarEditor,
        operation: 'GET',
        allowEntityIdEdit: true,
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        pageSize: 10,
        visibleToolbarButtons: {
            separator: false,
            attribute: false,
            relationship: false,
            context: false,
            validate: false,
            format: false,
            lineNumbers: false,
            coloring: false
        },
        onOperation: async (editor, operation, entityId, type) => {
            if (operation !== 'GET') return;

            try {
                const client = new QuantumLeapClient();
                const data = await client.getEntityTimeSeries(
                    entityObj.EntityID, 
                    entityObj.entityType, 
                    entityObj.DateTimeFrom, 
                    entityObj.DateTimeTo
                );
                console.log('[Debug] Received Quantum Leap entity data:', data);

                if (data) {
                    editor.setValue(JSON.stringify(data, null, 2));
                    appendToLogs(`Successfully fetched time series data for entity ${entityObj.EntityID}`);
                }
            } catch (error) {
                console.error('[Debug] Error fetching Quantum Leap entity:', error);
                appendToLogs(`Error fetching entity data: ${error.message}`);
                editor.setValue('[]');
            }
        }
    };

    // Create a new tab with the editor
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    console.log('[Debug] Created Quantum Leap entity tab:', { tabId });
}

/**
 * Opens a table view of all entities grouped by type in a new tab
 */
export async function openFormEntityAllByTypeTableView() {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = 'Entities By Type';
    const editorOptions = {
        mode: 'table',
        showSearch: true,
        showPagination: true,
        showColumnToggle: true,
        data: [],
        editorClass: FormEntityAllByTypeTableToolbarEditor
    };
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    const client = new OrionLDSearchClient();
    let typeRows = [];
    try {
        // Fetch all types
        const types = await client.getAllTypes();
        // For each type, fetch entities and count them
        for (const type of types) {
            const entities = await client.getEntitiesByType(type);
            typeRows.push({
                type: type,
                count: Array.isArray(entities) ? entities.length : 0
            });
        }
    } catch (e) {
        alert('Failed to fetch entities by type: ' + e);
        return;
    }
    // Update the editor in the tab with type/count rows
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.editor && typeof tab.editor.setValue === 'function') {
        tab.editor.setValue(typeRows);
        if (typeof tab.editor.updateDisplay === 'function') {
            tab.editor.updateDisplay();
        }
    }
}
window.openFormEntityAllByTypeTableView = openFormEntityAllByTypeTableView;

/**
 * Opens a table view of all entities of a specific type in a new tab
 * @param {string} EntityType - The entity type to fetch and display
 */
export async function openFormEntityAllOfTypeTableView(EntityType) {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = `Entities of Type: ${EntityType}`;
    const editorOptions = {
        mode: 'table',
        showSearch: true,
        showPagination: true,
        showColumnToggle: true,
        data: [],
        editorClass: FormEntityAllByTypeTableToolbarEditor
    };
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    const client = new OrionLDSearchClient();
    let entities = [];
    try {
        entities = await client.getEntitiesByType(EntityType);
    } catch (e) {
        alert('Failed to fetch entities of type ' + EntityType + ': ' + e);
        return;
    }
    // Update the editor in the tab with the entities
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.editor && typeof tab.editor.setValue === 'function') {
        tab.editor.setValue(entities);
        if (typeof tab.editor.updateDisplay === 'function') {
            tab.editor.updateDisplay();
        }
    }
}
window.openFormEntityAllOfTypeTableView = openFormEntityAllOfTypeTableView;

/**
 * Opens a table view for a specific entity in a new tab
 * @param {string|null} EntityId - The entity ID to fetch and display, or null for empty editor
 */
export async function openFormEntityTableView(EntityId) {
    console.debug('[UIHandler] openFormEntityTableView called with EntityId:', EntityId);
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = EntityId ? `Entity: ${EntityId}` : 'Entity Editor';
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormEntityTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true, // Always visible
            relationship: true, // Always visible
            context: true, // Always visible
            toggle: false // Hide the toggle button json/table
        },
        operation: ['GET', 'PUT', 'PATCH', 'DELETE'],
        allowEntityIdEdit: true,
        entityId: EntityId || ''
    };
    let entity = null;
    if (EntityId) {
        const { OrionLDClient } = await import('./api.js');
        const client = new OrionLDClient();
        try {
            entity = await client.getEntity(EntityId);
            console.debug('[UIHandler] Entity fetched from OrionLDClient:', entity);
            editorOptions.initialValue = JSON.stringify(entity, null, 2);
        } catch (e) {
            alert('Failed to fetch entity: ' + e);
            return;
        }
    }
    console.debug('[UIHandler] Creating editor tab with options:', editorOptions);
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    if (EntityId && entity) {
        // Set value when editor is ready
        const tab = window.tabManager.getTabById(tabId);
        if (tab && tab.contentElement) {
            const onReady = (e) => {
                console.debug('[UIHandler] editorReady event fired for FormEntityTableToolbarEditor', e.detail.editor);
                e.detail.editor.setValue(entity);
                console.debug('[UIHandler] setValue called on FormEntityTableToolbarEditor with:', entity);
                if (typeof e.detail.editor.updateDisplay === 'function') {
                    e.detail.editor.updateDisplay();
                    console.debug('[UIHandler] updateDisplay called on FormEntityTableToolbarEditor');
                }
                tab.contentElement.removeEventListener('editorReady', onReady);
            };
            tab.contentElement.addEventListener('editorReady', onReady);
        } else {
            console.warn('[UIHandler] Tab or contentElement not found for tabId:', tabId);
        }
    }
}

/**
 * Opens a table view for creating a new entity in POST mode
 */
export async function openFormEntityPostTableView() {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = 'Entity POST';
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormEntityTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true,
            relationship: true,
            context: true,
            toggle: false
        },
        operation: ['POST'],
        allowEntityIdEdit: true,
        entityId: '',
    };
    // Provide a template for a new entity
    const entityTemplate = {
        id: 'urn:ngsi-ld:Entity:12345',
        type: 'EntityType',
        attribute1: 'value1',
        attribute2: 'value2'
    };
    editorOptions.initialValue = JSON.stringify(entityTemplate, null, 2);
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    // Set value when editor is ready
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.contentElement) {
        const onReady = (e) => {
            e.detail.editor.setValue(entityTemplate);
            if (typeof e.detail.editor.updateDisplay === 'function') {
                e.detail.editor.updateDisplay();
            }
            tab.contentElement.removeEventListener('editorReady', onReady);
        };
        tab.contentElement.addEventListener('editorReady', onReady);
    }
}
window.openFormEntityTableView = openFormEntityTableView;

/**
 * Opens a table view of all subscriptions in a new tab, with search, pagination, and column toggle
 */
export async function openFormSubscriptionAllOfTypeTableView() {
    // Ensure TabManager is initialized
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    // Dynamic import for editor and API client
    const [{ default: FormSubscriptionAllTableToolbarEditor }, { SubscriptionClient }] = await Promise.all([
        import('./FormSubscriptionAllTableToolbarEditor.js'),
        import('./api.js')
    ]);
    const tabTitle = 'All Subscriptions';
    const editorOptions = {
        mode: 'table',
        showSearch: true,
        showPagination: true,
        showColumnToggle: true,
        data: [],
        editorClass: FormSubscriptionAllTableToolbarEditor
    };
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    const client = new SubscriptionClient();
    let subscriptions = [];
    try {
        subscriptions = await client.getAll();
    } catch (e) {
        alert('Failed to fetch subscriptions: ' + e);
        return;
    }
    // Update the editor in the tab with the subscriptions
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.editor && typeof tab.editor.setValue === 'function') {
        tab.editor.setValue(subscriptions);
        if (typeof tab.editor.updateDisplay === 'function') {
            tab.editor.updateDisplay();
        }
    }
}
window.openFormSubscriptionAllOfTypeTableView = openFormSubscriptionAllOfTypeTableView;

/**
 * Opens a table view of all DataProducts in a new tab, with search, pagination, and column toggle
 */
export async function openFormDataProductAllOfTypeTableView() {
    // Ensure TabManager is initialized
    if (!window.tabManager) {
        try {
            const { TabManager } = await import('./TabManager.js');
            window.tabManager = await TabManager.initialize();
        } catch (e) {
            alert('Failed to initialize TabManager: ' + e);
            return;
        }
    }
    // Dynamic import for editor and API client
    let FormDataProductAllTableToolbarEditor, DataProductClient;
    try {
        const editorModule = await import('./FormDataProductAllTableToolbarEditor.js');
        FormDataProductAllTableToolbarEditor = editorModule.default;
        DataProductClient = (await import('./api.js')).DataProductClient;
    } catch (e) {
        alert('Failed to load required modules: ' + e);
        return;
    }
    const tabTitle = 'All DataProducts';
    const editorOptions = {
        mode: 'table',
        showSearch: true,
        showPagination: true,
        showColumnToggle: true,
        data: [],
        editorClass: FormDataProductAllTableToolbarEditor
    };
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    const client = new DataProductClient();
    let dataProducts = [];
    try {
        dataProducts = await client.getAll();
    } catch (e) {
        alert('Failed to fetch DataProducts: ' + e);
        return;
    }
    // Update the editor in the tab with the DataProducts
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.editor && typeof tab.editor.setValue === 'function') {
        tab.editor.setValue(dataProducts);
        if (typeof tab.editor.updateDisplay === 'function') {
            tab.editor.updateDisplay();
        }
    }
}
window.openFormDataProductAllOfTypeTableView = openFormDataProductAllOfTypeTableView;

/**
 * Opens a table view for creating a new Subscription in POST mode
 */
export async function openFormSubscriptionPostTableView() {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = 'Subscription POST';
    let FormSubscriptionTableToolbarEditor;
    try {
        FormSubscriptionTableToolbarEditor = (await import('./FormSubscriptionTableToolbarEditor.js')).default;
    } catch (e) {
        alert('Failed to load FormSubscriptionTableToolbarEditor: ' + e);
        return;
    }
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormSubscriptionTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true,
            relationship: true,
            context: true,
            toggle: false
        },
        operation: ['POST'],
        allowEntityIdEdit: true,
        entityId: '',
    };
    // Provide a template for a new subscription
    const subscriptionTemplate = {
        id: 'urn:ngsi-ld:Subscription:12345',
        type: 'Subscription',
        entities: [{ type: 'EntityType' }],
        notification: {
            endpoint: { uri: 'http://example.com/notify', accept: 'application/json' }
        },
        watchedAttributes: ['attribute1']
    };
    editorOptions.initialValue = JSON.stringify(subscriptionTemplate, null, 2);
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    // Set value when editor is ready
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.contentElement) {
        const onReady = (e) => {
            e.detail.editor.setValue(subscriptionTemplate);
            if (typeof e.detail.editor.updateDisplay === 'function') {
                e.detail.editor.updateDisplay();
            }
            tab.contentElement.removeEventListener('editorReady', onReady);
        };
        tab.contentElement.addEventListener('editorReady', onReady);
    }
}
window.openFormSubscriptionPostTableView = openFormSubscriptionPostTableView;

/**
 * Opens a table view for creating a new DataProduct in POST mode
 */
export async function openFormDataProductPostTableView() {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = 'DataProduct POST';
    let FormDataProductTableToolbarEditor;
    try {
        FormDataProductTableToolbarEditor = (await import('./FormDataProductTableToolbarEditor.js')).default;
    } catch (e) {
        alert('Failed to load FormDataProductTableToolbarEditor: ' + e);
        return;
    }
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormDataProductTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true,
            relationship: true,
            context: true,
            toggle: false
        },
        operation: ['POST'],
        allowEntityIdEdit: true,
        entityId: '',
    };
    // Provide a template for a new DataProduct
    const dataProductTemplate = {
        id: 'urn:ngsi-ld:DataProduct:12345',
        type: 'DataProduct',
        name: 'Example DataProduct',
        description: 'A sample DataProduct for demonstration.',
        attributes: {
            attribute1: 'value1',
            attribute2: 'value2'
        }
    };
    editorOptions.initialValue = JSON.stringify(dataProductTemplate, null, 2);
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    // Set value when editor is ready
    const tab = window.tabManager.getTabById(tabId);
    if (tab && tab.contentElement) {
        const onReady = (e) => {
            e.detail.editor.setValue(dataProductTemplate);
            if (typeof e.detail.editor.updateDisplay === 'function') {
                e.detail.editor.updateDisplay();
            }
            tab.contentElement.removeEventListener('editorReady', onReady);
        };
        tab.contentElement.addEventListener('editorReady', onReady);
    }
}
window.openFormDataProductPostTableView = openFormDataProductPostTableView;

/**
 * Opens a table view for a specific Subscription in a new tab
 * @param {string|null} SubscriptionId - The Subscription ID to fetch and display, or null for empty editor
 */
export async function openFormSubscriptionTableView(SubscriptionId) {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = SubscriptionId ? `Subscription: ${SubscriptionId}` : 'Subscription Editor';
    let FormSubscriptionTableToolbarEditor, SubscriptionClient;
    try {
        FormSubscriptionTableToolbarEditor = (await import('./FormSubscriptionTableToolbarEditor.js')).default;
        SubscriptionClient = (await import('./api.js')).SubscriptionClient;
    } catch (e) {
        alert('Failed to load required modules: ' + e);
        return;
    }
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormSubscriptionTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true,
            relationship: true,
            context: true,
            toggle: false
        },
        operation: ['GET', 'PUT', 'PATCH', 'DELETE'],
        allowEntityIdEdit: true,
        entityId: SubscriptionId || ''
    };
    let subscription = null;
    if (SubscriptionId) {
        const client = new SubscriptionClient();
        try {
            subscription = await client.getById(SubscriptionId);
            editorOptions.initialValue = JSON.stringify(subscription, null, 2);
        } catch (e) {
            alert('Failed to fetch subscription: ' + e);
            return;
        }
    }
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    if (SubscriptionId && subscription) {
        // Set value when editor is ready
        const tab = window.tabManager.getTabById(tabId);
        if (tab && tab.contentElement) {
            const onReady = (e) => {
                e.detail.editor.setValue(subscription);
                if (typeof e.detail.editor.updateDisplay === 'function') {
                    e.detail.editor.updateDisplay();
                }
                tab.contentElement.removeEventListener('editorReady', onReady);
            };
            tab.contentElement.addEventListener('editorReady', onReady);
        }
    }
}
window.openFormSubscriptionTableView = openFormSubscriptionTableView;

/**
 * Opens a table view for a specific DataProduct in a new tab
 * @param {string|null} DataProductId - The DataProduct ID to fetch and display, or null for empty editor
 */
export async function openFormDataProductTableView(DataProductId) {
    if (!window.tabManager) {
        const { TabManager } = await import('./TabManager.js');
        window.tabManager = await TabManager.initialize();
    }
    const tabTitle = DataProductId ? `DataProduct: ${DataProductId}` : 'DataProduct Editor';
    let FormDataProductTableToolbarEditor, DataProductClient;
    try {
        FormDataProductTableToolbarEditor = (await import('./FormDataProductTableToolbarEditor.js')).default;
        DataProductClient = (await import('./api.js')).DataProductClient;
    } catch (e) {
        alert('Failed to load required modules: ' + e);
        return;
    }
    const editorOptions = {
        mode: 'json',
        showSearch: false,
        showPagination: false,
        showColumnToggle: false,
        data: {},
        editorClass: FormDataProductTableToolbarEditor,
        visibleToolbarButtons: {
            validate: true,
            format: true,
            lineNumbers: true,
            coloring: true,
            attribute: true,
            relationship: true,
            context: true,
            toggle: false
        },
        operation: ['GET', 'PUT', 'PATCH', 'DELETE'],
        allowEntityIdEdit: true,
        entityId: DataProductId || ''
    };
    let dataProduct = null;
    if (DataProductId) {
        const client = new DataProductClient();
        try {
            dataProduct = await client.getById(DataProductId);
            editorOptions.initialValue = JSON.stringify(dataProduct, null, 2);
        } catch (e) {
            alert('Failed to fetch DataProduct: ' + e);
            return;
        }
    }
    const tabId = window.tabManager.createEditorTab(tabTitle, editorOptions);
    if (DataProductId && dataProduct) {
        // Set value when editor is ready
        const tab = window.tabManager.getTabById(tabId);
        if (tab && tab.contentElement) {
            const onReady = (e) => {
                e.detail.editor.setValue(dataProduct);
                if (typeof e.detail.editor.updateDisplay === 'function') {
                    e.detail.editor.updateDisplay();
                }
                tab.contentElement.removeEventListener('editorReady', onReady);
            };
            tab.contentElement.addEventListener('editorReady', onReady);
        }
    }
}
window.openFormDataProductTableView = openFormDataProductTableView;

// Make functions available globally
window.initializeUI = initializeUI;


window.openMenuItemInTab = openMenuItemInTab;



window.openFormQuantumLeapTableView = openFormQuantumLeapTableView;
window.openFormQuantumLeapEntityView = openFormQuantumLeapEntityView;

window.openFormEntityAllByTypeTableView = openFormEntityAllByTypeTableView;
window.openFormEntityAllOfTypeTableView = openFormEntityAllOfTypeTableView;
window.openFormEntityTableView = openFormEntityTableView;
window.openFormEntityPostTableView = openFormEntityPostTableView;

window.openFormSubscriptionAllOfTypeTableView = openFormSubscriptionAllOfTypeTableView;
window.openFormSubscriptionPostTableView = openFormSubscriptionPostTableView;
window.openFormSubscriptionTableView = openFormSubscriptionTableView;

window.openFormDataProductAllOfTypeTableView = openFormDataProductAllOfTypeTableView;
window.openFormDataProductPostTableView = openFormDataProductPostTableView;
window.openFormDataProductTableView = openFormDataProductTableView;

export default {
    initializeUI,
    toggleTreeView,
    initTreeView,
    openMenuItemInTab,
    openFormQuantumLeapTableView,
    openFormQuantumLeapEntityView,
    openFormEntityAllByTypeTableView,
    openFormEntityAllOfTypeTableView,
    openFormEntityTableView,
    openFormEntityPostTableView,
    openFormSubscriptionAllOfTypeTableView,
    openFormDataProductAllOfTypeTableView,
    openFormSubscriptionPostTableView,
    openFormDataProductPostTableView,
    openFormSubscriptionTableView,
    openFormDataProductTableView
};
