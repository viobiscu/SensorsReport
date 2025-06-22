// TableRenderer.js
// TableRenderer - Responsible for rendering tables and related helpers

class TableRenderer {
    /**
     * Display data in table format
     * @param {Array} data Array of objects to display in table
     * @param {HTMLElement} container The container to render the table into
     * @param {Object} [options] Optional settings (e.g., groupByType)
     */
    displayTable(data, container, options = {}) {
        if (!Array.isArray(data) || data.length === 0) {
            container.innerHTML = '<div class="empty-table">No data to display</div>';
            return;
        }
        container.innerHTML = '';
        if (options.groupByType) {
            const groups = this.groupEntitiesByType(data);
            Object.entries(groups).forEach(([type, entities]) => {
                const typeHeader = document.createElement('h3');
                typeHeader.textContent = `${type} (${entities.length})`;
                typeHeader.style.marginTop = '20px';
                typeHeader.style.marginBottom = '10px';
                typeHeader.style.padding = '8px';
                typeHeader.style.backgroundColor = 'var(--card-bg)'; // Theme variable
                typeHeader.style.color = 'var(--text-color)'; // Theme variable
                typeHeader.style.border = '1px solid var(--border-color)'; // Theme variable
                typeHeader.style.borderRadius = '4px';
                container.appendChild(typeHeader);
                this.createEntityTable(entities, container, options);
            });
        } else {
            this.createEntityTable(data, container, options);
        }
    }

    /**
     * Create a table for a set of entities
     * @param {Array} entities Array of entities to display
     * @param {HTMLElement} container The container to render the table into
     * @param {Object} [options] Optional settings (e.g., sortState, onSort, onReverseSelection)
     */
    createEntityTable(entities, container, options = {}) {
        console.log('[TableRenderer] createEntityTable called with entities:', entities);
        const columns = new Set();
        const columnWidths = new Map();
        const getTextWidth = (text) => {
            const canvas = document.createElement('canvas');
            const context = canvas.getContext('2d');
            context.font = '13px monospace';
            return context.measureText(String(text || '')).width;
        };
        entities.forEach((item, idx) => {
            if (typeof item === 'object' && item !== null) {
                Object.entries(item).forEach(([key, value]) => {
                    columns.add(key);
                    const headerWidth = getTextWidth(this.formatColumnName(key)) + 32;
                    let contentWidth = 80;
                    if (value && typeof value === 'object') {
                        if (value.type === 'Property' && 'value' in value) {
                            contentWidth = getTextWidth(value.value) + 16;
                        } else {
                            contentWidth = getTextWidth(JSON.stringify(value)) + 16;
                        }
                    } else {
                        contentWidth = getTextWidth(value) + 16;
                    }
                    const currentWidth = columnWidths.get(key) || 0;
                    columnWidths.set(key, Math.min(300, Math.max(80, currentWidth, headerWidth, contentWidth)));
                });
            }
        });
        console.log('[TableRenderer] Columns detected:', Array.from(columns));

        let sortState = { column: null, direction: 1 };
        if (options.sortState) sortState = options.sortState;
        let sortedEntities = [...entities];
        if (sortState.column) {
            sortedEntities.sort((a, b) => {
                const valA = a[sortState.column];
                const valB = b[sortState.column];
                if (valA === valB) return 0;
                if (valA === undefined) return 1;
                if (valB === undefined) return -1;
                return (valA > valB ? 1 : -1) * sortState.direction;
            });
        }

        const table = document.createElement('table');
        table.className = 'json-table';
        table.style.width = '100%';
        table.style.tableLayout = 'fixed';
        table.style.borderCollapse = 'collapse';
        table.style.fontFamily = 'monospace';
        table.style.fontSize = '13px';
        table.style.color = 'var(--text-color)'; // Theme variable for default text color

        const thead = document.createElement('thead');
        const headerRow = document.createElement('tr');
        headerRow.style.backgroundColor = 'var(--table-header-bg)'; // Theme variable

        // Add checkbox header as first column
        const thCheckbox = document.createElement('th');
        thCheckbox.style.width = '32px';
        thCheckbox.style.minWidth = '32px';
        thCheckbox.style.maxWidth = '32px';
        thCheckbox.style.textAlign = 'center';
        thCheckbox.style.backgroundColor = 'var(--table-header-bg)'; // Match header background
        thCheckbox.style.borderBottom = '2px solid var(--table-border)'; // Theme variable, consistent with other th

        const masterCheckbox = document.createElement('input');
        masterCheckbox.type = 'checkbox';
        masterCheckbox.title = 'Reverse Selection';
        thCheckbox.appendChild(masterCheckbox);
        headerRow.appendChild(thCheckbox);

        Array.from(columns).forEach(column => {
            const th = document.createElement('th');
            th.textContent = this.formatColumnName(column);
            th.style.padding = '8px';
            th.style.borderBottom = '2px solid var(--table-border)'; // Theme variable
            th.style.textAlign = 'left';
            th.style.fontWeight = 'bold';
            th.style.color = 'var(--text-color)'; // Theme variable for header text
            th.style.position = 'relative';
            th.style.overflow = 'hidden';
            th.style.textOverflow = 'ellipsis';
            th.style.whiteSpace = 'nowrap';
            th.style.width = `${columnWidths.get(column)}px`;
            th.style.minWidth = '80px';
            th.style.maxWidth = '300px';
            if (th.textContent !== column) {
                th.title = column;
            }
            // Add sort icon
            th.style.cursor = 'pointer';
            const sortIcon = document.createElement('span');
            sortIcon.style.marginLeft = '4px';
            sortIcon.textContent = sortState.column === column ? (sortState.direction === 1 ? '▲' : '▼') : '';
            th.appendChild(sortIcon);
            th.addEventListener('click', () => {
                if (options.onSort) options.onSort(column, sortState.column === column ? -sortState.direction : 1);
            });
            // Add resizer
            const resizer = document.createElement('div');
            resizer.style.position = 'absolute';
            resizer.style.right = '0';
            resizer.style.top = '0';
            resizer.style.width = '5px';
            resizer.style.height = '100%';
            resizer.style.cursor = 'col-resize';
            resizer.addEventListener('mousedown', (e) => {
                e.preventDefault();
                const startX = e.pageX;
                const startWidth = th.offsetWidth;
                const onMouseMove = (moveEvent) => {
                    const newWidth = Math.max(40, startWidth + (moveEvent.pageX - startX));
                    th.style.width = newWidth + 'px';
                };
                const onMouseUp = () => {
                    document.removeEventListener('mousemove', onMouseMove);
                    document.removeEventListener('mouseup', onMouseUp);
                };
                document.addEventListener('mousemove', onMouseMove);
                document.addEventListener('mouseup', onMouseUp);
            });
            th.appendChild(resizer);
            headerRow.appendChild(th);
        });

        thead.appendChild(headerRow);
        table.appendChild(thead);
        const tbody = document.createElement('tbody');
        sortedEntities.forEach((item, index) => {
            const row = document.createElement('tr');
            // REMOVED: row.style.backgroundColor - Handled by CSS for .json-table tbody tr:nth-child(odd/even) td

            // Add checkbox as first cell
            const tdCheckbox = document.createElement('td');
            tdCheckbox.style.textAlign = 'center';
            tdCheckbox.style.width = '32px';
            tdCheckbox.style.minWidth = '32px';
            tdCheckbox.style.maxWidth = '32px';
            // Note: Borders and background for tdCheckbox are handled by style.css
            // e.g., .json-table td:first-child and tr:nth-child(odd/even) td:first-child

            const rowCheckbox = document.createElement('input');
            rowCheckbox.type = 'checkbox';
            rowCheckbox.className = 'row-checkbox';
            tdCheckbox.appendChild(rowCheckbox);
            row.appendChild(tdCheckbox);
            Array.from(columns).forEach(column => {
                const td = document.createElement('td');
                td.style.padding = '8px';
                // REMOVED: td.style.borderBottom - Handled by style.css .json-table td { border: 1px solid var(--table-border); }
                td.style.overflow = 'hidden';
                td.style.textOverflow = 'ellipsis';
                td.style.whiteSpace = 'nowrap';
                let value = item[column];
                let rawValue = value;
                // Improved NGSI-LD value extraction
                if (value && typeof value === 'object') {
                    if (value.type === 'Property' && 'value' in value) {
                        value = value.value;
                    } else if ('value' in value) {
                        value = value.value;
                    } else {
                        value = JSON.stringify(value);
                    }
                }
                td.textContent = value !== undefined ? value : '';
                td.title = td.textContent;
                row.appendChild(td);
                // Detailed log for each cell
                if (index < 3) {
                    console.log('[TableRenderer] Row', index, 'Column', column, 'Raw value:', rawValue, 'Rendered value:', value);
                }
            });
            tbody.appendChild(row);
            if (index < 3) {
                console.log('[TableRenderer] Rendered row', index, item);
            }
        });
        table.appendChild(tbody);
        container.appendChild(table);

        console.log('[TableRenderer] Total rows rendered:', sortedEntities.length);

        // Reverse selection logic
        masterCheckbox.addEventListener('click', (e) => {
            // Only trigger reverse selection if the user clicks directly on the checkbox, not when sorting
            if (e.target === masterCheckbox) {
                if (options.onReverseSelection) options.onReverseSelection();
            }
        });
    }

    /**
     * Format column name for display
     * @param {string} column The raw column name
     * @returns {string} The formatted column name
     */
    formatColumnName(column) {
        let formatted = column.replace(/^(ngsi-|urn:)/, '');
        formatted = formatted.split(/[-_.]/)
            .map(word => word.charAt(0).toUpperCase() + word.slice(1).toLowerCase())
            .join(' ');
        if (column.toLowerCase() === 'id' || column.toLowerCase() === 'urn') {
            return column.toUpperCase();
        }
        return formatted;
    }

    /**
     * Group entities by their type
     * @param {Array} data Array of entities to group
     * @returns {Object} Grouped entities
     */
    groupEntitiesByType(data) {
        const groups = {};
        data.forEach(entity => {
            const type = entity.type || 'Unknown';
            if (!groups[type]) {
                groups[type] = [];
            }
            groups[type].push(entity);
        });
        return groups;
    }
}

export default TableRenderer;
