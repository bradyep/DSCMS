// ContentTypes Edit Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const id = pathParts[pathParts.length - 1];

  try {
    const [ctResponse, optionsResponse] = await Promise.all([
      fetch(`/api/contenttypes/${id}`),
      fetch('/api/contenttypes/form-options')
    ]);

    if (!ctResponse.ok) throw new Error('Failed to load content type');
    if (!optionsResponse.ok) throw new Error('Failed to load form options');

    const ct = await ctResponse.json();
    const options = await optionsResponse.json();

    app.innerHTML = `
      <h2>Edit Content Type</h2>
      <form id="editForm" class="form-horizontal">
        <div class="form-group">
          <label for="name" class="col-md-2 control-label">Name</label>
          <div class="col-md-10">
            <input type="text" id="name" class="form-control" value="${ct.name || ''}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="title" class="col-md-2 control-label">Title</label>
          <div class="col-md-10">
            <input type="text" id="title" class="form-control" value="${ct.title || ''}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="description" class="col-md-2 control-label">Description</label>
          <div class="col-md-10">
            <textarea id="description" class="form-control" rows="3">${ct.description || ''}</textarea>
          </div>
        </div>
        <div class="form-group">
          <label for="itemsPerPage" class="col-md-2 control-label">Items Per Page</label>
          <div class="col-md-10">
            <input type="number" id="itemsPerPage" class="form-control" value="${ct.itemsPerPage}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="multipleContentsTemplateId" class="col-md-2 control-label">Multiple Contents Template</label>
          <div class="col-md-10">
            <select id="multipleContentsTemplateId" class="form-control" required>
              ${options.multipleContentsTemplates
                .map(t => `<option value="${t.id}" ${ct.multipleContentsTemplateId === t.id ? 'selected' : ''}>${t.name}</option>`)
                .join('')}
            </select>
          </div>
        </div>
        <div class="form-group">
          <label for="defaultSingleContentTemplateId" class="col-md-2 control-label">Default Single Content Template</label>
          <div class="col-md-10">
            <select id="defaultSingleContentTemplateId" class="form-control">
              ${options.singleContentTemplates
                .map(t => `<option value="${t.id}" ${((ct.defaultSingleContentTemplateId ?? 0) === t.id) ? 'selected' : ''}>${t.name}</option>`)
                .join('')}
            </select>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-10 col-md-offset-2">
            <div class="checkbox">
              <label><input type="checkbox" id="isDefaultContentType" ${ct.isDefaultContentType ? 'checked' : ''} /> Is Default Content Type</label>
            </div>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <button type="submit" class="btn btn-primary">Save</button>
            <a href="/Admin/ContentTypes" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('editForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const defaultSingleId = parseInt(document.getElementById('defaultSingleContentTemplateId').value, 10);

      const data = {
        name: document.getElementById('name').value,
        title: document.getElementById('title').value,
        description: document.getElementById('description').value,
        itemsPerPage: parseInt(document.getElementById('itemsPerPage').value, 10),
        multipleContentsTemplateId: parseInt(document.getElementById('multipleContentsTemplateId').value, 10),
        defaultSingleContentTemplateId: defaultSingleId > 0 ? defaultSingleId : null,
        isDefaultContentType: document.getElementById('isDefaultContentType').checked
      };

      try {
        const updateResponse = await fetch(`/api/contenttypes/${id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!updateResponse.ok) {
          const errorData = await updateResponse.json();
          throw new Error(errorData.title || 'Failed to update content type');
        }

        window.location.href = '/Admin/ContentTypes';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
