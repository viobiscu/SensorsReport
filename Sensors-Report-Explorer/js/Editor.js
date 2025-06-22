/**
 * Editor Component
 * This provides core editor functionality without toolbar features
 */

class Editor {
    /**
     * Initialize an editor with the given configuration
     * @param {Object} config Configuration options
     * @param {string} config.containerId ID of the container element for the editor
     * @param {string} [config.initialValue] Initial string value
     * @param {number} [config.height=300] Height of the editor in pixels
     * @param {boolean} [config.resize=true] Whether the editor should be resizable
     * @param {boolean} [config.showLineNumbers=false] Whether to show line numbers
     * @param {Function} [config.onChange] Callback when content changes
     * @param {Function} [config.onSave] Callback when content is saved/formatted
     * @param {Object} [config.schema] JSON Schema for validation
     */
    constructor(config) {
        console.log('Editor constructor called with config:', config);
        this.containerId = config.containerId;
        this.container = document.getElementById(this.containerId);
        
        if (!this.container) {
            console.error(`Container element with ID "${this.containerId}" not found`);
            return;
        }
        
        this.height = config.height || 300;
        this.resizable = config.resize !== false;
        this.lineNumbersEnabled = config.showLineNumbers || false;
        this.onChange = config.onChange || (() => {});
        this.onSave = config.onSave || (() => {});
        this.coloringEnabled = true;
        this.schema = config.schema || null;
        this.debounceTimeoutId = null;
        this.MAX_VALIDATABLE_SIZE = 500000;
        
        // Create the editor structure
        this.createEditorElements();
        
        // Set initial value if provided
        if (config.initialValue) {
            this.setValue(config.initialValue);
        }
        
        // Set up event handlers
        this.setupEventHandlers();
        console.log('Editor base class initialization complete');
    }

    /**
     * Create the editor structure
     */
    createEditorElements() {
        // Create the main editor container
        this.editorContainer = document.createElement('div');
        this.editorContainer.className = 'editor-container';
        this.editorContainer.style.width = '100%';
        this.editorContainer.style.height = this.height + 'px';
        this.editorContainer.style.border = '1px solid #ccc';
        this.editorContainer.style.borderRadius = '4px';
        this.editorContainer.style.overflow = 'hidden';
        
        // Make the editor resizable if configured
        if (this.resizable) {
            this.editorContainer.style.resize = 'vertical';
        }
        
        // Create editing area container to position editor and line numbers properly
        this.editingArea = document.createElement('div');
        this.editingArea.className = 'editor-editing-area';
        this.editingArea.style.position = 'relative';
        this.editingArea.style.height = '100%';
        this.editorContainer.appendChild(this.editingArea);
        
        // Create line numbers container if enabled
        if (this.lineNumbersEnabled) {
            this.lineNumbersContainer = document.createElement('div');
            this.lineNumbersContainer.className = 'editor-line-numbers';
            this.lineNumbersContainer.style.position = 'absolute';
            this.lineNumbersContainer.style.top = '0';
            this.lineNumbersContainer.style.left = '0';
            this.lineNumbersContainer.style.width = '40px';
            this.lineNumbersContainer.style.height = '100%';
            this.lineNumbersContainer.style.backgroundColor = '#f5f5f5';
            this.lineNumbersContainer.style.borderRight = '1px solid #ddd';
            this.lineNumbersContainer.style.overflow = 'hidden';
            this.lineNumbersContainer.style.color = '#999';
            this.lineNumbersContainer.style.fontFamily = 'monospace';
            this.lineNumbersContainer.style.fontSize = '12px';
            this.lineNumbersContainer.style.userSelect = 'none';
            this.lineNumbersContainer.style.zIndex = '2';
            this.editingArea.appendChild(this.lineNumbersContainer);
        }
        
        // Create main editor wrapper
        this.editorWrapper = document.createElement('div');
        this.editorWrapper.className = 'editor-wrapper';
        this.editorWrapper.style.position = 'relative';
        this.editorWrapper.style.height = '100%';
        this.editorWrapper.style.paddingLeft = this.lineNumbersEnabled ? '40px' : '0';
        this.editorWrapper.style.boxSizing = 'border-box';
        
        // Create textarea for input
        this.textarea = document.createElement('textarea');
        this.textarea.className = 'editor-textarea';
        this.textarea.style.width = '100%';
        this.textarea.style.height = '100%';
        this.textarea.style.border = 'none';
        this.textarea.style.resize = 'none';
        this.textarea.style.outline = 'none';
        this.textarea.style.fontFamily = 'monospace';
        this.textarea.style.fontSize = '13px';
        this.textarea.style.padding = '8px';
        this.textarea.style.margin = '0';
        this.textarea.style.backgroundColor = 'transparent';
        this.textarea.style.position = 'absolute';
        this.textarea.style.top = '0';
        this.textarea.style.left = '0';
        this.textarea.style.color = 'transparent';
        this.textarea.style.caretColor = '#000';
        this.textarea.style.whiteSpace = 'pre-wrap';
        this.textarea.style.overflow = 'auto';
        this.textarea.style.zIndex = '1';
        this.textarea.spellcheck = false;
        
        // Create pre element for syntax highlighting
        this.displayContainer = document.createElement('pre');
        this.displayContainer.className = 'editor-display-container';
        this.displayContainer.style.width = '100%';
        this.displayContainer.style.height = '100%';
        this.displayContainer.style.margin = '0';
        this.displayContainer.style.padding = '8px';
        this.displayContainer.style.fontFamily = 'monospace';
        this.displayContainer.style.fontSize = '13px';
        this.displayContainer.style.backgroundColor = 'transparent';
        this.displayContainer.style.whiteSpace = 'pre-wrap';
        this.displayContainer.style.wordWrap = 'break-word';
        this.displayContainer.style.overflow = 'hidden';
        this.displayContainer.style.position = 'absolute';
        this.displayContainer.style.top = '0';
        this.displayContainer.style.left = '0';
        this.displayContainer.style.pointerEvents = 'none';
        
        // Create code element for syntax highlighting
        this.displayCode = document.createElement('code');
        this.displayCode.className = 'language-json';
        this.displayContainer.appendChild(this.displayCode);
        
        // Add elements to DOM
        this.editorWrapper.appendChild(this.textarea);
        this.editorWrapper.appendChild(this.displayContainer);
        this.editingArea.appendChild(this.editorWrapper);
        
        this.container.appendChild(this.editorContainer);
    }

    /**
     * Set up event handlers for the editor
     */
    setupEventHandlers() {
        // Synchronize scrolling between textarea and display
        this.textarea.addEventListener('scroll', this.synchronizeScrolling);
        
        // Textarea content changes
        this.textarea.addEventListener('input', this.handleInput);
        
        // Handle tab key
        this.textarea.addEventListener('keydown', this.handleKeyDown);
        
        // Add double-click handler to select text between quotes
        this.textarea.addEventListener('dblclick', this.handleDoubleClick);
        
        // When textarea loses focus, reformat content
        this.textarea.addEventListener('blur', () => this.format());
        
        // Window resize handler for editor resizing
        window.addEventListener('resize', this.handleResize);
        
        // Handle editor container resizing
        if (this.resizable) {
            this.editorContainer.addEventListener('mouseup', this.handleResize);
        }
    }

    /**
     * Handle input event on textarea
     */
    handleInput = () => {
        this.updateDisplay();
        
        // Add debounce for validation on input
        clearTimeout(this.debounceTimeoutId);
        this.debounceTimeoutId = setTimeout(() => {
            this.onChange(this.getValue());
        }, 300);
    }

    /**
     * Update the display (syntax highlighting) based on the textarea content 
     */
    updateDisplay = () => {
        const value = this.textarea.value;
        this.displayCode.innerHTML = this.coloringEnabled ? this.highlightSyntax(value) : this.escapeHtml(value);
        
        // Update line numbers if enabled
        if (this.lineNumbersEnabled) {
            this.updateLineNumbers();
        }
    }

    /**
     * Highlight syntax based on content
     * @param {string} content The content to highlight
     * @returns {string} HTML with syntax highlighting
     */
    highlightSyntax(content) {
        if (content.trim() === '') {
            return '';
        }

        try {
            // If syntax highlighting library is available (like Prism.js or highlight.js),
            // use it for better highlighting
            if (window.Prism && typeof Prism.highlight === 'function') {
                return Prism.highlight(content, Prism.languages.json, 'json');
            } else if (window.hljs && typeof hljs.highlight === 'function') {
                const result = hljs.highlight(content, { language: 'json' });
                return result.value;
            } else {
                // Simple own implementation of syntax highlighting
                return this.basicHighlight(content);
            }
        } catch (e) {
            console.warn('Error during syntax highlighting:', e);
            return this.escapeHtml(content);
        }
    }

    /**
     * Basic syntax highlighting with regex
     * Used as a fallback when no syntax highlighting library is available
     * @param {string} content The content to highlight
     * @returns {string} HTML with syntax highlighting
     */
    basicHighlight(content) {
        // Escape HTML first
        const escaped = this.escapeHtml(content);
        
        // Apply color to string values
        let highlighted = escaped.replace(/"([^"\\]*(\\.[^"\\]*)*)"(?=\s*:)/g, '<span style="color:#9c27b0;">"$1"</span>'); // Key
        highlighted = highlighted.replace(/:(?=\s*)"([^"\\]*(\\.[^"\\]*)*)"(?=[\s,}\]\n])/g, ': <span style="color:#0d47a1;">"$1"</span>'); // Value
        
        // Apply color to numbers
        highlighted = highlighted.replace(/([^":\w])(\-?\d+(\.\d+)?(?=[,\s\]\}]))/g, '$1<span style="color:#f57c00;">$2</span>');
        
        // Apply color to booleans and null
        highlighted = highlighted.replace(/(?<!["-])(true|false|null)(?![\w"-])/g, '<span style="color:#2e7d32;">$1</span>');
        
        // Apply color to brackets and braces
        highlighted = highlighted.replace(/([{}\[\]])/g, '<span style="color:#616161;">$1</span>');
        
        return highlighted;
    }

    /**
     * Escape HTML special characters
     * @param {string} html The HTML string to escape
     * @returns {string} The escaped HTML string
     */
    escapeHtml(html) {
        return html
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }

    /**
     * Update line numbers container with current line numbers
     */
    updateLineNumbers = () => {
        if (!this.lineNumbersEnabled || !this.lineNumbersContainer) {
            return;
        }
        
        const text = this.textarea.value;
        const lines = text.split('\n');
        const lineCount = lines.length;
        
        // Clear previous line numbers
        this.lineNumbersContainer.innerHTML = '';
        
        // Create a fragment to improve performance
        const fragment = document.createDocumentFragment();
        
        // Calculate approximate line height
        const baseLineHeight = parseInt(getComputedStyle(this.textarea).fontSize, 10) * 1.2;
        
        // Create each line number element
        for (let i = 0; i < lineCount; i++) {
            const lineNumberElement = document.createElement('div');
            lineNumberElement.textContent = i + 1;
            lineNumberElement.style.padding = '0 8px';
            
            // Calculate line height based on content
            const lineContent = lines[i];
            const lineWidth = this.displayContainer.clientWidth - 16;
            const textWidth = this.getTextWidth(lineContent);
            const wrappedLines = Math.max(1, Math.ceil(textWidth / lineWidth));
            
            lineNumberElement.style.height = (baseLineHeight * wrappedLines) + 'px';
            lineNumberElement.style.display = 'flex';
            lineNumberElement.style.alignItems = 'flex-start';
            lineNumberElement.style.justifyContent = 'flex-end';
            lineNumberElement.style.paddingTop = '0';
            
            fragment.appendChild(lineNumberElement);
        }
        
        this.lineNumbersContainer.appendChild(fragment);
    }

    /**
     * Estimate text width in pixels
     * @param {string} text The text to measure
     * @returns {number} Estimated width in pixels
     */
    getTextWidth(text) {
        if (!text) return 0;
        
        // Use canvas for measuring text width
        const canvas = this.getTextWidth.canvas || (this.getTextWidth.canvas = document.createElement('canvas'));
        const context = canvas.getContext('2d');
        
        context.font = getComputedStyle(this.textarea).font;
        return context.measureText(text).width;
    }

    /**
     * Synchronize scrolling between textarea and display elements
     */
    synchronizeScrolling = () => {
        this.displayContainer.scrollTop = this.textarea.scrollTop;
        this.displayContainer.scrollLeft = this.textarea.scrollLeft;
        
        if (this.lineNumbersEnabled && this.lineNumbersContainer) {
            this.lineNumbersContainer.scrollTop = this.textarea.scrollTop;
        }
    }

    /**
     * Handle keydown event (for tab key behavior)
     * @param {KeyboardEvent} e The keydown event
     */
    handleKeyDown = (e) => {
        if (e.key === 'Tab') {
            e.preventDefault();
            
            const start = this.textarea.selectionStart;
            const end = this.textarea.selectionEnd;
            
            const spaces = '  ';
            this.textarea.value = this.textarea.value.substring(0, start) + spaces + this.textarea.value.substring(end);
            
            this.textarea.selectionStart = this.textarea.selectionEnd = start + spaces.length;
            this.updateDisplay();
        }
        
        if ((e.ctrlKey || e.metaKey) && e.key === 's') {
            e.preventDefault();
            this.format();
            
            if (typeof this.onSave === 'function') {
                this.onSave(this.getValue());
            }
        }
    }

    /**
     * Handle double-click event to select text between quotes
     * @param {MouseEvent} e The double-click event
     */
    handleDoubleClick = (e) => {
        const value = this.textarea.value;
        const start = this.textarea.selectionStart;
        const end = this.textarea.selectionEnd;

        const before = value.lastIndexOf('"', start - 1);
        const after = value.indexOf('"', end);

        if (before !== -1 && after !== -1) {
            this.textarea.selectionStart = before + 1;
            this.textarea.selectionEnd = after;
        }
    }

    /**
     * Handle resize events
     */
    handleResize = () => {
        this.updateDisplay();
        
        if (this.editorContainer && this.editorContainer.parentElement) {
            const parentHeight = this.editorContainer.parentElement.clientHeight;
            if (parentHeight > 0) {
                this.editorContainer.style.height = `${parentHeight}px`;
            }
        }
    }

    /**
     * Set the editor's value
     * @param {string|Object} value String or object to set as the editor value
     */
    setValue(value) {
        let content;
        
        if (typeof value === 'object') {
            try {
                content = JSON.stringify(value, null, 2);
            } catch (e) {
                console.error('Error stringifying object:', e);
                content = '';
            }
        } else if (typeof value === 'string') {
            content = value;
        } else {
            content = String(value);
        }
        
        this.textarea.value = content;
        this.updateDisplay();
    }
    
    /**
     * Get the current value from the editor
     * @param {boolean} [parse=false] Whether to parse and return as object
     * @returns {string|Object} The current value
     */
    getValue(parse = false) {
        const value = this.textarea.value;
        
        if (parse) {
            try {
                return JSON.parse(value);
            } catch (e) {
                console.error('Error parsing content:', e);
                return null;
            }
        }
        
        return value;
    }

    /**
     * Format the current content
     * @return {boolean} Whether the operation succeeded
     */
    format() {
        const value = this.textarea.value;
        
        if (value.trim() === '') {
            return true;
        }
        
        try {
            const parsed = JSON.parse(value);
            const formatted = JSON.stringify(parsed, null, 2);
            
            if (formatted !== value) {
                this.textarea.value = formatted;
                this.updateDisplay();
            }
            
            return true;
        } catch (error) {
            // Silently fail on format errors
            return false;
        }
    }
}

// Add module exports
window.Editor = Editor;
export default Editor;