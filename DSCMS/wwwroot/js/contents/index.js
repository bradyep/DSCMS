// Index page - list all contents
(async function() {
  try {
    // Get filter from URL query string
    const urlParams = new URLSearchParams(window.location.search);
    const contentTypeFilter = urlParams.get('contentType') || '';

    // Load content types for filter dropdown
    const formOptionsResponse = await fetch('/api/contents/form-options');
    const formOptions = await formOptionsResponse.json();

    const filterSelect = document.getElementById('contentTypeFilter');
    filterSelect.innerHTML = '<option value="">All</option>';
    formOptions.contentTypes.forEach(ct => {
      const option = document.createElement('option');
      option.value = ct.name;
      option.textContent = ct.name;
      if (ct.name === contentTypeFilter) {
        option.selected = true;
      }
      filterSelect.appendChild(option);
    });

    // Load contents
    const url = contentTypeFilter 
      ? `/api/contents?contentType=${encodeURIComponent(contentTypeFilter)}`
      : '/api/contents';

    const response = await fetch(url);
    const contents = await response.json();

    const tbody = document.getElementById('contentsTableBody');
    tbody.innerHTML = '';

    if (contents.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-center">No contents found</td></tr>';
      return;
    }

    contents.forEach(content => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${escapeHtml(content.title || '')}</td>
        <td>${escapeHtml(content.contentTypeName || '')}</td>
        <td>${escapeHtml(content.urlToDisplay || '')}</td>
        <td>${content.bodySourceHasContent ? 'True' : 'False'}</td>
        <td>${new Date(content.lastUpdatedDate).toLocaleDateString()}</td>
        <td>
          <a href="/Admin/Contents/Edit/${content.contentId}">Edit</a> |
          <a href="/Admin/Contents/Details/${content.contentId}">Details</a> |
          <a href="/Admin/Contents/Delete/${content.contentId}">Delete</a>
        </td>
      `;
      tbody.appendChild(row);
    });

  } catch (error) {
    console.error('Error loading contents:', error);
    document.getElementById('contentsTableBody').innerHTML = 
      '<tr><td colspan="6" class="text-danger">Error loading contents. Please try again.</td></tr>';
  }
})();

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}
