// Edit page - edit existing content
(async function() {
  try {
    const contentId = document.getElementById('contentIdHolder').dataset.contentid;

    // Load form options and content data in parallel
    const [optionsResponse, contentResponse] = await Promise.all([
      fetch('/api/contents/form-options'),
      fetch(`/api/contents/${contentId}`)
    ]);

    if (!contentResponse.ok) {
      alert('Content not found');
      window.location.href = '/Admin/Contents';
      return;
    }

    const options = await optionsResponse.json();
    const content = await contentResponse.json();

    // Populate dropdowns
    populateSelect('ContentTypeId', options.contentTypes, content.contentTypeId);
    populateSelect('TemplateId', options.templates, content.templateId);
    populateSelect('BodySourceTypeId', options.sourceTypes, content.bodySourceTypeId);

    // Populate form fields
    document.getElementById('BodySource').value = content.bodySource || '';
    document.getElementById('Title').value = content.title || '';
    document.getElementById('UrlToDisplay').value = content.urlToDisplay || '';

    // Populate field items table if present
    if (content.contentTypeFieldItems && content.contentTypeFieldItems.length > 0) {
      const tbody = document.getElementById('fieldItemsTableBody');
      tbody.innerHTML = '';
      content.contentTypeFieldItems.forEach(item => {
        const row = document.createElement('tr');
        row.innerHTML = `
          <td>${escapeHtml(item.fieldName || '')}</td>
          <td>${escapeHtml(item.value || '')}</td>
          <td>
            <a href="/Admin/ContentTypeFieldItems/Edit/${item.contentTypeFieldItemId}">Edit</a> |
            <a href="/Admin/ContentTypeFieldItems/Delete/${item.contentTypeFieldItemId}">Delete</a>
          </td>
        `;
        tbody.appendChild(row);
      });
    }

  } catch (error) {
    console.error('Error loading content:', error);
    alert('Error loading content data. Please try again.');
  }
})();

function populateSelect(elementId, items, selectedValue) {
  const select = document.getElementById(elementId);
  select.innerHTML = '';
  items.forEach(item => {
    const option = document.createElement('option');
    option.value = item.id;
    option.textContent = item.name;
    if (item.id == selectedValue || item.name == selectedValue) {
      option.selected = true;
    }
    select.appendChild(option);
  });
}

document.getElementById('editForm').addEventListener('submit', async function(e) {
  e.preventDefault();

  const contentId = document.getElementById('contentIdHolder').dataset.contentid;

  const formData = {
    bodySource: document.getElementById('BodySource').value,
    bodySourceTypeId: parseInt(document.getElementById('BodySourceTypeId').value),
    contentTypeId: parseInt(document.getElementById('ContentTypeId').value),
    templateId: parseInt(document.getElementById('TemplateId').value),
    title: document.getElementById('Title').value,
    urlToDisplay: document.getElementById('UrlToDisplay').value
  };

  try {
    const response = await fetch(`/api/contents/${contentId}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(formData)
    });

    if (response.ok) {
      window.location.href = '/Admin/Contents';
    } else {
      const error = await response.json();
      alert('Error updating content: ' + (error?.title || JSON.stringify(error)));
    }
  } catch (error) {
    console.error('Error updating content:', error);
    alert('Error updating content. Please try again.');
  }
});

function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}
