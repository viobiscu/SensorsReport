// PaginationManager.js
// PaginationManager - Handles pagination controls and logic
import ButtonEditor from './ButtonEditor.js'; // ADDED: Import ButtonEditor

class PaginationManager {
    constructor({ pageSize = 10, currentPage = 1, onPageChange = null } = {}) {
        this.pageSize = pageSize;
        this.currentPage = currentPage;
        this.onPageChange = onPageChange; // Callback when page changes
        this.pageSizeSelect = null;
        this.prevButton = null;
        this.nextButton = null;
        this.pageInfo = null;
        this.buttonEditor = new ButtonEditor(); // ADDED: Instantiate ButtonEditor
    }

    /**
     * Add pagination controls to the container
     * @param {HTMLElement} container The container to add the pagination controls to
     */
    addPaginationControls(container) {
        const paginationWrapper = document.createElement('div');
        paginationWrapper.className = 'table-pagination-wrapper';
        paginationWrapper.style.display = 'flex';
        paginationWrapper.style.alignItems = 'center';
        paginationWrapper.style.gap = '5px';

        // Page size selector
        const pageSizeLabel = document.createElement('label');
        pageSizeLabel.textContent = 'Rows:';

        this.pageSizeSelect = document.createElement('select');
        this.applyInputStyle(this.pageSizeSelect);
        [10, 25, 50, 100].forEach(size => {
            const option = document.createElement('option');
            option.value = size;
            option.textContent = size;
            this.pageSizeSelect.appendChild(option);
        });
        this.pageSizeSelect.value = this.pageSize;

        // Navigation buttons
        this.prevButton = this.buttonEditor.createButton({
            icon: 'fas fa-chevron-left',
            title: 'Previous Page',
            isOperation: false
        });

        this.pageInfo = document.createElement('span');
        this.pageInfo.textContent = `Page ${this.currentPage}`;
        this.pageInfo.style.margin = '0 10px';

        this.nextButton = this.buttonEditor.createButton({
            icon: 'fas fa-chevron-right',
            title: 'Next Page',
            isOperation: false
        });

        // Add event listeners
        this.pageSizeSelect.addEventListener('change', () => {
            this.pageSize = parseInt(this.pageSizeSelect.value);
            this.handlePageChange(1);
        });

        this.prevButton.addEventListener('click', () => {
            this.handlePageChange(this.currentPage - 1);
        });

        this.nextButton.addEventListener('click', () => {
            this.handlePageChange(this.currentPage + 1);
        });

        // Add elements to wrapper
        paginationWrapper.appendChild(pageSizeLabel);
        paginationWrapper.appendChild(this.pageSizeSelect);
        paginationWrapper.appendChild(this.prevButton);
        paginationWrapper.appendChild(this.pageInfo);
        paginationWrapper.appendChild(this.nextButton);
        container.appendChild(paginationWrapper);
    }

    /**
     * Handle page changes
     * @param {number} newPage The page number to change to
     */
    handlePageChange(newPage) {
        if (newPage < 1) return;
        this.currentPage = newPage;
        if (this.pageInfo) this.pageInfo.textContent = `Page ${this.currentPage}`;
        if (this.prevButton) this.prevButton.disabled = this.currentPage === 1;
        if (typeof this.onPageChange === 'function') {
            this.onPageChange({ page: this.currentPage, pageSize: this.pageSize });
        }
    }

    /**
     * Render pagination controls into a container
     * @param {HTMLElement} container The container to render pagination controls into
     * @param {Object} options Options for rendering (e.g., total, onPageChange)
     */
    render(container, options = {}) {
        // Remove previous controls if any
        container.innerHTML = '';
        if (options.total !== undefined) {
            this.total = options.total;
        }
        if (options.onPageChange) {
            this.onPageChange = ({ page, pageSize }) => options.onPageChange(page);
        }
        this.addPaginationControls(container);
        // Update page info and button states
        if (this.pageInfo) this.pageInfo.textContent = `Page ${this.currentPage}`;
        if (this.prevButton) this.prevButton.disabled = this.currentPage === 1;
        if (this.nextButton && this.total !== undefined) {
            const maxPage = Math.ceil(this.total / this.pageSize);
            this.nextButton.disabled = this.currentPage >= maxPage;
        }
    }

    /**
     * Apply consistent styling to input elements
     * @param {HTMLElement} input The input element to style
     */
    applyInputStyle(input) {
        input.style.padding = '4px 8px';
        input.style.borderRadius = '3px';
        input.style.border = '1px solid #ccc';
        input.style.fontSize = '12px';
    }
}

export default PaginationManager;
