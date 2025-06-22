
/* Tab Manager for JsonEditor instances */
class TabManager {
    constructor(containerSelector) {
        // Store tab data - Initialize these first before calling init()
        this.tabs = [];
        this.activeTabId = null;
        this.tabCounter = 0;
        
        this.container = document.querySelector(containerSelector);
        if (!this.container) {
            throw new Error(`Container element not found: ${containerSelector}`);
        }
        
        // Initialize the tab container structure
        this.init();
    }
    
    /**
     * Shorten an entity ID for tab display
     * @param {string} id - The full entity ID
     * @returns {string} - Shortened ID
     */
    shortenId(id) {
        if (!id) return id;
        
        // If it's a URN, extract the last part
        if (id.includes(':')) {
            const parts = id.split(':');
            return parts[parts.length - 1];
        }
        
        // If it's too long, truncate it
        if (id.length > 15) {
            return id.substring(0, 12) + '...';
        }
        
        return id;
    }
    
    /**
     * Detect and abbreviate entity ID in title
     * @param {string} title - The original title
     * @returns {string} - Title with abbreviated entity ID
     */
    abbreviateEntityId(title) {
        // Check if title contains an entity ID (typically in format "OPERATION urn:ngsi-ld:...")
        if (title && title.includes('urn:ngsi-ld:')) {
            // Extract entity ID by finding the URN pattern
            const match = title.match(/(urn:ngsi-ld:[^:]+:[^:\s]+)/i);
            if (match && match[1]) {
                const fullId = match[1];
                const shortId = this.shortenId(fullId);
                // Replace full ID with short ID in the title
                return title.replace(fullId, shortId);
            }
        }
        return title;
    }
    
    init() {
        // Create tab bar and content container
        this.tabsContainer = document.createElement('div');
        this.tabsContainer.className = 'editor-tabs-container';
        this.tabsContainer.style.background = 'var(--content-bg, #fff)';
        this.tabsContainer.style.color = 'var(--text-color, #222)';
        this.tabsContainer.style.border = '1px solid var(--border-color, #ccc)';
        this.tabsContainer.style.borderRadius = '8px';
        this.tabsContainer.style.overflow = 'hidden';

        // Tab header bar
        this.tabBar = document.createElement('div');
        this.tabBar.className = 'editor-tabs-bar';
        this.tabBar.style.display = 'flex';
        this.tabBar.style.background = 'var(--toolbar-bg, #f5f5f5)';
        this.tabBar.style.borderBottom = '1px solid var(--border-color, #ccc)';
        this.tabBar.style.padding = '0 4px';
        this.tabBar.style.height = '40px';
        this.tabBar.style.alignItems = 'center';
        this.tabBar.style.gap = '2px';
        this.tabBar.style.overflowX = 'auto';

        // Tab content area
        this.contentArea = document.createElement('div');
        this.contentArea.className = 'editor-tabs-content';
        this.contentArea.style.background = 'var(--content-bg, #fff)';
        this.contentArea.style.color = 'var(--text-color, #222)';
        this.contentArea.style.height = 'calc(100% - 40px)';
        this.contentArea.style.overflow = 'auto';
        this.contentArea.style.position = 'relative';

        // Assemble the components
        this.tabsContainer.appendChild(this.tabBar);
        this.tabsContainer.appendChild(this.contentArea);

        // Replace the container's contents with our tab structure
        this.container.innerHTML = '';
        this.container.appendChild(this.tabsContainer);
    }
    
    createTab(title, id = null, options = {}) {
        // Generate a unique ID if not provided
        const tabId = id || `tab-${++this.tabCounter}`;
        
        // Abbreviate entity ID in title
        let abbreviatedTitle = this.abbreviateEntityId(title);
        
        // Truncate to max 20 characters
        if (abbreviatedTitle.length > 20) {
            abbreviatedTitle = abbreviatedTitle.slice(0, 17) + '...';
        }
        
        // Create tab element
        const tab = document.createElement('div');
        tab.className = 'editor-tab';
        tab.style.background = 'var(--tab-bg, transparent)';
        tab.style.color = 'var(--text-color, #222)';
        tab.style.borderRadius = '6px 6px 0 0';
        tab.style.marginRight = '2px';
        tab.style.padding = '0 16px';
        tab.style.height = '36px';
        tab.style.display = 'flex';
        tab.style.alignItems = 'center';
        tab.style.cursor = 'pointer';
        tab.style.userSelect = 'none';
        tab.style.position = 'relative';
        tab.style.transition = 'background 0.2s';
        tab.style.fontWeight = '500';
        tab.style.fontSize = '15px';
        tab.style.maxWidth = '220px';
        tab.style.overflow = 'hidden';
        tab.style.textOverflow = 'ellipsis';
        tab.style.whiteSpace = 'nowrap';

        if (options.mode === 'welcome') {
            tab.classList.add('welcome-tab');
            tab.style.background = 'var(--tab-welcome-bg, #e9f5ff)';
            tab.style.color = 'var(--primary-color, #007bff)';
        }

        tab.dataset.tabId = tabId;

        // Create title span with ellipsis for overflow
        const titleSpan = document.createElement('span');
        titleSpan.className = 'tab-title';
        titleSpan.textContent = abbreviatedTitle;
        titleSpan.style.overflow = 'hidden';
        titleSpan.style.textOverflow = 'ellipsis';
        titleSpan.style.whiteSpace = 'nowrap';
        titleSpan.style.maxWidth = '160px';
        tab.appendChild(titleSpan);
        
        // Only add close button if not welcome tab
        if (!options.mode || options.mode !== 'welcome') {
            const closeBtn = document.createElement('span');
            closeBtn.innerHTML = '&times;';
            closeBtn.className = 'tab-close-btn';
            closeBtn.style.marginLeft = '10px';
            closeBtn.style.fontSize = '18px';
            closeBtn.style.color = 'var(--icon-color, #888)';
            closeBtn.style.cursor = 'pointer';
            closeBtn.style.transition = 'color 0.2s';
            closeBtn.addEventListener('mouseover', () => {
                closeBtn.style.color = 'var(--danger-color, #dc3545)';
            });
            closeBtn.addEventListener('mouseout', () => {
                closeBtn.style.color = 'var(--icon-color, #888)';
            });
            closeBtn.addEventListener('click', (e) => {
                e.stopPropagation(); // Prevent tab activation
                this.closeTab(tabId);
            });
            
            tab.appendChild(closeBtn);
        }
        
        // Add click event for tab
        tab.addEventListener('click', () => {
            this.activateTab(tabId);
        });
        
        // Add to tab bar
        this.tabBar.appendChild(tab);
        
        return tab;
    }
    
    createEditorTab(title, options) {
        console.log('[Debug] Creating editor tab with options:', {
            title,
            editorClass: options.editorClass?.name,
            mode: options.mode,
            showToolbar: options.showToolbar
        });

        // Generate a unique tab ID
        const tabId = `editor-${++this.tabCounter}`;
        
        // Create the tab
        const tab = this.createTab(title, tabId);
        
        // Create content container for this tab
        const contentContainer = document.createElement('div');
        contentContainer.className = 'editor-tab-content';
        contentContainer.id = `${tabId}-content`;
        contentContainer.style.height = '100%';
        contentContainer.style.display = 'none'; // Hide initially
        
        // Create editor container inside the content
        const editorContainer = document.createElement('div');
        editorContainer.id = `${tabId}-editor`;
        editorContainer.style.height = '100%';
        contentContainer.appendChild(editorContainer);
        
        // Add the content to the content area
        this.contentArea.appendChild(contentContainer);
        
        // Create the editor using the specified editor class or default to JsonEditor
        options.containerId = editorContainer.id;
        options.container = editorContainer.id; // Ensure compatibility with TableToolbarEditorModular
        const EditorClass = options.editorClass ;
        console.log('[Debug] Initializing editor with class:', {
            className: EditorClass.name,
            containerId: options.containerId,
            initialValue: options.initialValue?.substring(0, 100) + '...'
        });
        const editor = new EditorClass(options);
        
        // Store tab information
        this.tabs.push({
            id: tabId,
            tabElement: tab,
            contentElement: contentContainer,
            editor: editor,
            mode: options.mode || 'standard'
        });

        // Log the created tab and editor state
        console.log('[Debug] Created editor tab:', { 
            tabId,
            editorMode: editor.viewMode,
            hasTableContainer: !!editor.tableContainer,
            editorType: editor.constructor.name
        });
        
        // Activate the new tab
        this.activateTab(tabId);
        
        return tabId;
    }
    
    activateTab(tabId) {
        // Skip if already active
        if (this.activeTabId === tabId) return;
        
        // Deactivate current active tab
        if (this.activeTabId) {
            const activeTab = this.tabs.find(t => t.id === this.activeTabId);
            if (activeTab) {
                activeTab.tabElement.classList.remove('active');
                activeTab.tabElement.style.background = 'var(--tab-bg, transparent)';
                activeTab.tabElement.style.color = 'var(--text-color, #222)';
                activeTab.contentElement.style.display = 'none';
            }
        }
        
        // Activate the new tab
        const tab = this.tabs.find(t => t.id === tabId);
        if (tab) {
            tab.tabElement.classList.add('active');
            tab.tabElement.style.background = 'var(--tab-active-bg, #e6f0fa)';
            tab.tabElement.style.color = 'var(--primary-color, #007bff)';
            tab.contentElement.style.display = 'block';
            this.activeTabId = tabId;
            
            // Set window.mainEditor to the current tab's editor for backward compatibility
            if (tab.editor) {
                window.mainEditor = tab.editor;
            }
        }
    }
    
    closeTab(tabId) {
        // Find the tab index
        const tabIndex = this.tabs.findIndex(t => t.id === tabId);
        if (tabIndex === -1) return;
        
        const tab = this.tabs[tabIndex];
        
        // Prevent closing the welcome tab (regardless of tab count)
        if (tab.mode === 'welcome') return;
        
        // Remove DOM elements
        tab.tabElement.remove();
        tab.contentElement.remove();
        
        // If this was the active tab, activate another one
        if (this.activeTabId === tabId) {
            // Try to activate the tab to the left, or the first tab if none on the left
            let newTabId;
            if (tabIndex > 0) {
                newTabId = this.tabs[tabIndex - 1].id;
            } else if (this.tabs.length > 1) {
                newTabId = this.tabs[1].id;
            } else {
                newTabId = null;
            }
            
            // Remove the tab from the array first
            this.tabs.splice(tabIndex, 1);
            
            // Activate new tab if available
            if (newTabId) {
                this.activateTab(newTabId);
            } else {
                this.activeTabId = null;
                window.mainEditor = null;
            }
        } else {
            // Just remove the tab from the array
            this.tabs.splice(tabIndex, 1);
        }
    }
    
    closeAllTabs() {
        // Close all tabs except welcome
        const tabsToClose = this.tabs
            .filter(tab => tab.mode !== 'welcome')
            .map(tab => tab.id);
        
        tabsToClose.forEach(tabId => this.closeTab(tabId));
        
        // Ensure welcome tab is visible and active if it exists
        const welcomeTab = this.tabs.find(tab => tab.mode === 'welcome');
        if (welcomeTab) {
            this.activateTab(welcomeTab.id);
        }
    }
    
    getTabById(tabId) {
        return this.tabs.find(t => t.id === tabId);
    }
    
    getActiveTab() {
        return this.tabs.find(t => t.id === this.activeTabId);
    }
    
    getActiveTabEditor() {
        const activeTab = this.getActiveTab();
        return activeTab ? activeTab.editor : null;
    }

    /**
     * Initialize TabManager and create welcome tab
     * @returns {Promise<TabManager>} The initialized TabManager instance
     */
    static async initialize() {
        console.log('Initializing TabManager');
        
        // Create TabManager
        const tabManager = new TabManager('#displayArea');
        
        // Create a default welcome tab
        const welcomeTabId = `welcome-tab-${Date.now()}`;
        const welcomeTab = tabManager.createTab('Welcome', welcomeTabId, { mode: 'welcome' });
        
        // Create content container for welcome tab
        const contentContainer = document.createElement('div');
        contentContainer.className = 'editor-tab-content';
        contentContainer.id = `${welcomeTabId}-content`;
        contentContainer.style.height = '100%';
        contentContainer.style.display = 'none'; // Hide initially
        
        // Add welcome content
        contentContainer.innerHTML = `
            <div style="padding: 20px;">
                <h1>Welcome to SR Explorer</h1>
                <p>Select an option from the menu on the left to get started.</p>
            </div>
        `;
        
        // Add content container to tabs array
        tabManager.tabs.push({
            id: welcomeTabId,
            tabElement: welcomeTab,
            contentElement: contentContainer,
            mode: 'welcome'
        });
        
        // Add content to content area and activate tab
        tabManager.contentArea.appendChild(contentContainer);
        tabManager.activateTab(welcomeTabId);
        
        return tabManager;
    }
}

// Add module exports
window.TabManager = TabManager;
export { TabManager };
export default TabManager;