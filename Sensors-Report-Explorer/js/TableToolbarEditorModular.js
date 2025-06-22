// TableToolbarEditorModular.js
import TableRenderer from './TableRenderer.js';
import ToolbarManager from './ToolbarManager.js';
import PaginationManager from './PaginationManager.js';
import SelectionManager from './SelectionManager.js';
import Editor from './Editor.js';
import ButtonEditor from './ButtonEditor.js';

class TableToolbarEditorModular {
    constructor(config) {
        this.config = config;
        this.container = typeof config.container === 'string' ? document.getElementById(config.container) : config.container;
        this.mode = config.mode || 'table';
        // Use initialValue if present, otherwise fallback to config.data or []
        if (config.initialValue) {
            try {
                this.data = typeof config.initialValue === 'string' ? JSON.parse(config.initialValue) : config.initialValue;
            } catch (e) {
                console.warn('[TableToolbarEditorModular] Failed to parse initialValue, falling back to empty array:', e);
                this.data = [];
            }
        } else {
            this.data = config.data || [];
        }
        this.visibleColumns = null; // null = all visible
        this.columnVisibility = {}; // Track visibility by column key
        this.sortState = { column: null, direction: 1 };
        this.tableRenderer = new TableRenderer();
        this.toolbarManager = new ToolbarManager({ className: 'tte-modular-toolbar' });
        this.paginationManager = new PaginationManager({ pageSize: config.pageSize || 10 });
        this.selectionManager = null;
        this.editor = null;
        this.showSearch = config.showSearch !== undefined ? config.showSearch : true;
        this.showPagination = config.showPagination !== undefined ? config.showPagination : true;
        this.showColumnToggle = config.showColumnToggle !== undefined ? config.showColumnToggle : true;
        this.entityId = config.entityId || '';
        this.allowEntityIdEdit = config.allowEntityIdEdit !== undefined ? config.allowEntityIdEdit : true;
        this._searchQuery = '';

        // Normalize operation config to always be an array
        let op = config.operation || '';
        if (Array.isArray(op)) {
            this.operation = op;
        } else if (typeof op === 'string') {
            try {
                // Try to parse stringified array
                if (op.trim().startsWith('[') && op.trim().endsWith(']')) {
                    this.operation = JSON.parse(op.replace(/'/g, '"'));
                } else if (op.includes(',')) {
                    this.operation = op.split(',').map(s => s.trim());
                } else if (op.length > 0) {
                    this.operation = [op.trim()];
                } else {
                    this.operation = [];
                }
            } catch (e) {
                console.warn('[TableToolbarEditorModular] Failed to parse operation string:', op, e);
                this.operation = [];
            }
        } else {
            this.operation = [];
        }
        console.debug('[TableToolbarEditorModular] Normalized operation:', this.operation, 'Type:', typeof this.operation);

        this._init();
    }

    _init() {
        this.container.innerHTML = '';
        this.toolbarEl = document.createElement('div');
        this.toolbarEl.className = 'tte-modular-toolbar-container';
        this.container.appendChild(this.toolbarEl);

        this.contentEl = document.createElement('div');
        this.contentEl.className = 'tte-modular-content';
        this.container.appendChild(this.contentEl);

        this.paginationEl = document.createElement('div');
        this.paginationEl.className = 'tte-modular-pagination';
        this.container.appendChild(this.paginationEl);

        this._render();
    }

    _render() {
        this.toolbarEl.innerHTML = '';
        this.contentEl.innerHTML = '';
        this.paginationEl.innerHTML = '';
        if (this.mode === 'table') {
            this._renderTableToolbar();
            this._renderTable();
            this._renderPagination();
        } else {
            this._renderJsonToolbar();
            this._renderJsonEditor();
        }
    }

    _addNgsiButtonsHidden() {
        const buttonEditor = window.ButtonEditor ? new window.ButtonEditor() : new ButtonEditor();

        // Attribute button
        const attributeBtn = buttonEditor.createButton({
            title: 'Insert NGSI-LD Attribute',
            icon: 'fas fa-plus-circle'
        });
        attributeBtn.style.display = 'none';
        this.toolbarEl.appendChild(attributeBtn);

        // Relationship button
        const relationshipBtn = buttonEditor.createButton({
            title: 'Insert NGSI-LD Relationship',
            icon: 'fas fa-link'
        });
        relationshipBtn.style.display = 'none';
        this.toolbarEl.appendChild(relationshipBtn);

        // Context button
        const contextBtn = buttonEditor.createButton({
            title: 'Insert NGSI-LD Context',
            icon: 'fas fa-sitemap'
        });
        contextBtn.style.display = 'none';
        this.toolbarEl.appendChild(contextBtn);
    }

    _addNgsiButtons() {
        const cfg = this.config.visibleToolbarButtons || {};
        const buttonEditor = window.ButtonEditor ? new window.ButtonEditor() : new ButtonEditor();
        const addButton = (title, iconClass, onClickHandler, operationName) => {
            const button = buttonEditor.createButton({
                title,
                icon: iconClass,
            });
            button.classList.add('tte-ngsi-button', `tte-ngsi-${operationName}`);
            button.onclick = () => {
                if (typeof onClickHandler === 'function') onClickHandler();
            };
            this.toolbarEl.appendChild(button);
        };
        if (cfg.attribute) {
            addButton('Insert NGSI-LD Attribute', 'fas fa-plus-circle', this.onInsertAttribute, 'attribute');
        }
        if (cfg.relationship) {
            addButton('Insert NGSI-LD Relationship', 'fas fa-link', this.onInsertRelationship, 'relationship');
        }
        if (cfg.context) {
            addButton('Insert NGSI-LD Context', 'fas fa-sitemap', this.onInsertContext, 'context');
        }
    }

    _renderOperationButtons() {
        const buttonEditor = window.ButtonEditor ? new window.ButtonEditor() : new ButtonEditor();
        let ops = this.operation;
        if (Array.isArray(ops)) {
            if (ops.length === 0) {
                console.warn('[TableToolbarEditorModular] Operation array is empty, no buttons will be rendered.');
            }
            if (ops.some(op => ['GET', 'PATCH', 'PUT'].includes(op.toUpperCase()))) {
                const input = document.createElement('input');
                input.type = 'text';
                input.placeholder = 'Entity ID';
                input.value = this.entityId;
                input.disabled = !this.allowEntityIdEdit;
                input.className = 'tte-entity-input';
                this.toolbarEl.appendChild(input);
                input.addEventListener('input', (e) => {
                    this.entityId = e.target.value;
                });
            }
            ops.forEach(op => {
                const opName = op.toUpperCase();
                let icon = 'fas fa-cogs';
                let color = undefined;
                switch (opName) {
                    case 'GET': icon = 'fas fa-search'; color = '#28a745'; break;
                    case 'POST': icon = 'fas fa-plus'; color = '#007bff'; break;
                    case 'PUT': icon = 'fas fa-save'; color = '#17a2b8'; break;
                    case 'PATCH': icon = 'fas fa-edit'; color = '#ffc107'; break;
                    case 'DELETE': icon = 'fas fa-trash-alt'; color = '#dc3545'; break;
                }
                const btn = buttonEditor.createButton({
                    title: opName,
                    icon,
                    color,
                    isOperation: true
                });
                btn.classList.add('tte-operation-button', `tte-op-${opName.toLowerCase()}`);
                let handler = null;
                switch (opName) {
                    case 'GET': handler = this.onGet; break;
                    case 'POST': handler = this.onPost; break;
                    case 'PUT': handler = this.onPut; break;
                    case 'PATCH': handler = this.onPatch; break;
                    case 'DELETE': handler = this.onDelete; break;
                }
                if (typeof handler === 'function') {
                    btn.onclick = handler.bind(this);
                } else {
                    btn.disabled = true;
                    btn.title = opName + ' handler not implemented';
                }
                this.toolbarEl.appendChild(btn);
            });
        } else if (typeof ops === 'string' && ops) {
            const opName = ops.toUpperCase();
            if (opName === 'GET') {
                const input = document.createElement('input');
                input.type = 'text';
                input.placeholder = 'Entity ID';
                input.value = this.entityId;
                input.disabled = !this.allowEntityIdEdit;
                input.className = 'tte-entity-input';
                this.toolbarEl.appendChild(input);
                input.addEventListener('input', (e) => { this.entityId = e.target.value; });
            }
            let icon = 'fas fa-cogs';
            let color = undefined;
            if (opName === 'GET') { icon = 'fas fa-search'; color = '#28a745'; }
            else if (opName === 'POST') { icon = 'fas fa-plus'; color = '#007bff'; }
            const btn = buttonEditor.createButton({
                title: opName,
                icon,
                color,
                isOperation: true
            });
            btn.classList.add('tte-operation-button', `tte-op-${opName.toLowerCase()}`);
            if (opName === 'GET' && typeof this.onGet === 'function') btn.onclick = this.onGet.bind(this);
            else if (opName === 'POST' && typeof this.onPost === 'function') btn.onclick = this.onPost.bind(this);
            else { btn.disabled = true; btn.title = opName + ' handler not implemented'; }
            this.toolbarEl.appendChild(btn);
        } else {
            console.warn('[TableToolbarEditorModular] Operation is not set or not recognized:', ops);
        }
    }

    _renderTableToolbar() {
        this.toolbarEl.innerHTML = ''; // Clear the entire toolbar container first
        const buttonEditor = window.ButtonEditor ? new window.ButtonEditor() : new ButtonEditor();

        const showToggle = this.config.visibleToolbarButtons?.toggle !== false;

        if (showToggle) {
            const switchToJsonBtn = buttonEditor.createButton({
                icon: 'fas fa-code',
                title: 'Switch to JSON Editor'
            });
            switchToJsonBtn.classList.add('tte-toolbar-button', 'tte-toggle-button');
            switchToJsonBtn.onclick = () => { this.mode = 'json'; this._render(); };
            this.toolbarEl.appendChild(switchToJsonBtn);
        }

        if (this.showColumnToggle) {
            const selectColumnsBtn = buttonEditor.createButton({
                icon: 'fas fa-columns',
                title: 'Select Columns'
            });
            selectColumnsBtn.classList.add('tte-toolbar-button', 'tte-column-toggle-button');
            selectColumnsBtn.onclick = () => this._showColumnPopup();
            this.toolbarEl.appendChild(selectColumnsBtn);
        }
        
        // ToolbarManager might still be used by SelectionManager or other components
        // to add buttons. So, we clear it and render its container if it has content.
        this.toolbarManager.clear();
        const toolbarManagerContainer = document.createElement('div');
        // This allows ToolbarManager to render any buttons it's managing (e.g., from SelectionManager)
        this.toolbarManager.render(toolbarManagerContainer);
        if (toolbarManagerContainer.hasChildNodes()) {
            this.toolbarEl.appendChild(toolbarManagerContainer);
        }
        
        this._addNgsiButtons(); // Appends NGSI buttons (already uses ButtonEditor) to this.toolbarEl
        this._renderOperationButtons(); // Appends Op buttons (already uses ButtonEditor) to this.toolbarEl
        
        if (this.showSearch) {
            this._renderSearchBarInline(); // Appends search bar to this.toolbarEl
        }
    }

    _renderTable() {
        const pageData = this._getPageData();
        const tableContainer = document.createElement('div');
        tableContainer.className = 'tte-modular-table-container';
        this.contentEl.appendChild(tableContainer);
        let visibleColumns = null;
        if (this.data.length > 0) {
            const allColumns = Object.keys(this.data[0]);
            visibleColumns = allColumns.filter(col => this.columnVisibility[col] !== false);
        }
        this.tableRenderer.displayTable(
            pageData,
            tableContainer,
            {
                visibleColumns: visibleColumns,
                checkbox: true,
                sortState: this.sortState,
                onSort: (column, direction) => {
                    this.sortState = { column, direction };
                    this._render();
                },
                onReverseSelection: () => {
                    if (this.selectionManager) this.selectionManager.reverseSelection();
                },
                // Pass theme color variables for table styling
                theme: {
                    headerBg: 'var(--table-header-bg)',
                    rowOddBg: 'var(--table-odd-row-bg)',
                    rowEvenBg: 'var(--table-even-row-bg)',
                    hoverBg: 'var(--table-hover)',
                    selectedBg: 'var(--table-selected)',
                    border: 'var(--table-border)',
                    text: 'var(--text-color)',
                    contentBg: 'var(--content-bg)'
                }
            }
        );
        this.selectionManager = new SelectionManager({
            tableContainer,
            toolbar: this.toolbarEl,
            buttonEditor: this.toolbarManager,
            getValue: () => this.data,
            setValue: (val) => { this.data = JSON.parse(val); this._render(); }
        });
        this.selectionManager.attachCheckboxListeners();
    }

    _renderPagination() {
        if (!this.showPagination) {
            this.paginationEl.style.display = 'none';
            return;
        }
        this.paginationManager.render(this.paginationEl, {
            total: this.data.length,
            onPageChange: (page) => { this.paginationManager.currentPage = page; this._render(); }
        });
        this.paginationEl.style.display = '';
    }

    _renderJsonToolbar() {
        this.toolbarEl.innerHTML = ''; // Clear the entire toolbar container first
        const buttonEditor = window.ButtonEditor ? new window.ButtonEditor() : new ButtonEditor();

        const showToggle = this.config.visibleToolbarButtons?.toggle !== false;

        if (showToggle) {
            const switchToTableBtn = buttonEditor.createButton({
                icon: 'fas fa-table',
                title: 'Switch to Table View'
            });
            switchToTableBtn.classList.add('tte-toolbar-button', 'tte-toggle-button');
            switchToTableBtn.onclick = () => { this.mode = 'table'; this._render(); };
            this.toolbarEl.appendChild(switchToTableBtn);
        }

        const validateJsonBtn = buttonEditor.createButton({
            icon: 'fas fa-check',
            title: 'Validate JSON'
        });
        validateJsonBtn.classList.add('tte-toolbar-button', 'tte-json-validate-button');
        validateJsonBtn.onclick = () => this.editor && this._validateJson();
        this.toolbarEl.appendChild(validateJsonBtn);

        const formatJsonBtn = buttonEditor.createButton({
            icon: 'fas fa-align-left',
            title: 'Format JSON'
        });
        formatJsonBtn.classList.add('tte-toolbar-button', 'tte-json-format-button');
        formatJsonBtn.onclick = () => this.editor && this.editor.format();
        this.toolbarEl.appendChild(formatJsonBtn);

        const toggleLinesBtn = buttonEditor.createButton({
            icon: 'fas fa-list-ol',
            title: 'Toggle Line Numbers'
        });
        toggleLinesBtn.classList.add('tte-toolbar-button', 'tte-json-lines-button');
        toggleLinesBtn.onclick = () => this.editor && this._toggleLineNumbers();
        this.toolbarEl.appendChild(toggleLinesBtn);

        const toggleColoringBtn = buttonEditor.createButton({
            icon: 'fas fa-palette',
            title: 'Toggle Coloring'
        });
        toggleColoringBtn.classList.add('tte-toolbar-button', 'tte-json-color-button');
        toggleColoringBtn.onclick = () => this.editor && this._toggleColoring();
        this.toolbarEl.appendChild(toggleColoringBtn);
        
        // ToolbarManager might still be used by other components.
        this.toolbarManager.clear();
        const toolbarManagerContainer = document.createElement('div');
        this.toolbarManager.render(toolbarManagerContainer);
        if (toolbarManagerContainer.hasChildNodes()) {
            this.toolbarEl.appendChild(toolbarManagerContainer);
        }

        this._addNgsiButtons(); // Appends NGSI buttons to this.toolbarEl
        this._renderOperationButtons(); // Appends Op buttons to this.toolbarEl
        this.paginationEl.style.display = 'none';
    }

    _renderJsonEditor() {
        const editorId = 'tte-modular-json-editor-' + Math.random().toString(36).slice(2);
        const editorDiv = document.createElement('div');
        editorDiv.id = editorId;
        this.contentEl.appendChild(editorDiv);
        this.editor = new Editor({
            containerId: editorId,
            initialValue: JSON.stringify(this.data, null, 2),
            showLineNumbers: false,
            onChange: (val) => { try { this.data = JSON.parse(val); } catch {} },
        });
    }

    _getPageData() {
        let filtered = this.data;
        if (this.showSearch && this._searchQuery && this._searchQuery.trim()) {
            const q = this._searchQuery.trim().toLowerCase();
            filtered = this.data.filter(row =>
                Object.values(row).some(val =>
                    typeof val === 'object' ? JSON.stringify(val).toLowerCase().includes(q) : String(val).toLowerCase().includes(q)
                )
            );
        }
        if (!this.showPagination) return filtered;
        const pageSize = this.paginationManager.pageSize;
        const page = this.paginationManager.currentPage || 1;
        const start = (page - 1) * pageSize;
        const end = start + pageSize;
        return filtered.slice(start, end);
    }

    _renderSearchBarInline() {
        if (this.searchBarEl && this.searchBarEl.parentElement === this.toolbarEl) {
        } else {
            if (this.searchBarEl) this.searchBarEl.remove();
            this.searchBarEl = document.createElement('input');
            this.searchBarEl.type = 'search';
            this.searchBarEl.placeholder = 'Search...';
            this.searchBarEl.className = 'tte-search-input';
            this.toolbarEl.appendChild(this.searchBarEl);
        }
        
        this.searchBarEl.value = this._searchQuery || '';
        this.searchBarEl.removeEventListener('input', this._handleSearchInput);
        this._handleSearchInput = () => {
            this._searchQuery = this.searchBarEl.value;
            this._render();
        };
        this.searchBarEl.addEventListener('input', this._handleSearchInput);
    }

    _showColumnPopup() {
        const existingPopup = document.querySelector('.tte-column-popup');
        if (existingPopup) existingPopup.remove();

        const allColumns = this.data.length > 0 ? Object.keys(this.data[0]) : [];
        if (allColumns.length === 0) return;

        const popup = document.createElement('div');
        popup.className = 'tte-column-popup';
        popup.style.background = 'var(--content-bg)';
        popup.style.color = 'var(--text-color)';
        popup.style.border = '1px solid var(--border-color)';
        
        const header = document.createElement('div');
        header.className = 'tte-column-popup-header';
        header.textContent = 'Select Columns';
        popup.appendChild(header);

        const list = document.createElement('ul');
        list.className = 'tte-column-popup-list';

        allColumns.forEach(col => {
            const listItem = document.createElement('li');
            const label = document.createElement('label');
            const cb = document.createElement('input');
            cb.type = 'checkbox';
            cb.checked = this.columnVisibility[col] !== false;
            cb.onchange = () => {
                this.columnVisibility[col] = cb.checked;
                this._renderTable();
            };
            label.appendChild(cb);
            label.appendChild(document.createTextNode(' ' + col));
            listItem.appendChild(label);
            list.appendChild(listItem);
        });
        popup.appendChild(list);
        document.body.appendChild(popup);

        const columnButton = this.toolbarEl.querySelector('.tte-column-toggle-button .fa-columns');
        let buttonElement = columnButton ? columnButton.closest('button') : null;

        if (buttonElement) {
            const rect = buttonElement.getBoundingClientRect();
            popup.style.left = rect.left + 'px';
            popup.style.top = (rect.bottom + window.scrollY + 5) + 'px';
        } else {
            const containerRect = this.container.getBoundingClientRect();
            popup.style.left = (containerRect.left + 20) + 'px';
            popup.style.top = (containerRect.top + window.scrollY + 20) + 'px';
        }

        const closePopup = (e) => {
            if (!popup.contains(e.target) && e.target !== buttonElement && !buttonElement?.contains(e.target)) {
                popup.remove();
                document.removeEventListener('mousedown', closePopup);
            }
        };
        setTimeout(() => document.addEventListener('mousedown', closePopup), 0);
    }

    _validateJson() {
        try {
            JSON.parse(this.editor.getValue());
        } catch (e) {
            alert('Invalid JSON: ' + e.message);
        }
    }

    _toggleLineNumbers() {
        this.editor.lineNumbersEnabled = !this.editor.lineNumbersEnabled;
        this.editor.createEditorElements();
        this.editor.setValue(this.editor.getValue());
    }

    _toggleColoring() {
        this.editor.coloringEnabled = !this.editor.coloringEnabled;
        this.editor.updateDisplay();
    }
}

export default TableToolbarEditorModular;
