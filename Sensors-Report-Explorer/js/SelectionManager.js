// SelectionManager - Handles row selection and related UI logic
class SelectionManager {
    constructor({ tableContainer, toolbar, buttonEditor, getValue, setValue }) {
        this.tableContainer = tableContainer;
        this.toolbar = toolbar;
        this.buttonEditor = buttonEditor;
        this.getValue = getValue;
        this.setValue = setValue;
        this.selectedRows = new Set();
        this.deleteButton = null;
    }

    /**
     * Update selection UI
     */
    updateSelectionUI() {
        const rows = this.tableContainer.querySelectorAll('tbody tr');
        rows.forEach((row, index) => {
            row.style.backgroundColor = this.selectedRows.has(index)
                ? '#e3f2fd'
                : index % 2 === 0 ? '#f8f9fa' : 'white';
        });
        const table = this.tableContainer.querySelector('table');
        if (!table) return;
        const headerCheckbox = table.querySelector('thead input[type="checkbox"]');
        const rowCheckboxes = Array.from(table.querySelectorAll('tbody input[type="checkbox"]'));
        if (headerCheckbox && rowCheckboxes.length > 0) {
            const allChecked = rowCheckboxes.every(cb => cb.checked);
            const someChecked = rowCheckboxes.some(cb => cb.checked);
            headerCheckbox.checked = allChecked;
            headerCheckbox.indeterminate = someChecked && !allChecked;
        }
    }

    /**
     * Update delete button visibility
     */
    updateDeleteButtonVisibility() {
        if (!this.deleteButton) {
            this.deleteButton = this.buttonEditor.addButton({
                icon: 'fas fa-trash',
                title: 'Delete Selected',
                color: '#dc3545',
                isOperation: true
            });
            this.deleteButton.classList.add('delete-button');
            this.deleteButton.style.display = 'none';
            this.deleteButton.style.marginLeft = '8px';
            this.deleteButton.addEventListener('click', () => this.handleDelete());
        }
        this.deleteButton.style.display = this.selectedRows.size > 0 ? 'inline-flex' : 'none';
    }

    /**
     * Attach checkbox listeners for row selection
     */
    attachCheckboxListeners() {
        const table = this.tableContainer.querySelector('table');
        if (!table) return;
        const rowCheckboxes = Array.from(table.querySelectorAll('tbody .row-checkbox'));
        rowCheckboxes.forEach((cb, idx) => {
            cb.addEventListener('change', () => {
                if (cb.checked) {
                    this.selectedRows.add(idx);
                } else {
                    this.selectedRows.delete(idx);
                }
                this.updateSelectionUI();
                this.updateDeleteButtonVisibility();
            });
        });
        this.updateSelectionUI();
        this.updateDeleteButtonVisibility();
    }

    /**
     * Reverse selection when header checkbox is clicked
     */
    reverseSelection() {
        const table = this.tableContainer.querySelector('table');
        if (!table) return;
        const rowCheckboxes = Array.from(table.querySelectorAll('tbody .row-checkbox'));
        rowCheckboxes.forEach((cb, idx) => {
            cb.checked = !cb.checked;
            if (cb.checked) {
                this.selectedRows.add(idx);
            } else {
                this.selectedRows.delete(idx);
            }
        });
        this.updateSelectionUI();
        this.updateDeleteButtonVisibility();
    }

    /**
     * Handle delete action for selected rows
     */
    async handleDelete() {
        if (this.selectedRows.size === 0) return;
        const confirmMessage = `Are you sure you want to delete ${this.selectedRows.size} selected rows?`;
        if (!confirm(confirmMessage)) return;
        try {
            const data = this.getValue(true);
            if (!Array.isArray(data)) return;
            const newData = data.filter((_, index) => !this.selectedRows.has(index));
            this.setValue(JSON.stringify(newData, null, 2));
            this.selectedRows.clear();
            // Optionally, you may want to re-render the table here
        } catch (error) {
            console.error('Error handling delete:', error);
        }
    }
}

export default SelectionManager;
