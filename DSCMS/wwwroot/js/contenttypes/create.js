// ContentTypes Create Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    const optionsResponse = await fetch('/api/contenttypes/form-options');
    if (!optionsResponse.ok) throw new Error('Failed to load form options');
    const options = await optionsResponse.json();

    app.innerHTML = `
      <h2>Create Content Type</h2>
      <form id="createForm" class="form-horizontal">
        <div class="form-group">
          <label for="name" class="col-md-2 control-label">Name</label>
          <div class="col-md-10">
            <input type="text" id="name" class="form-control" required />
          </div>
        </div>
        <div class="form-group">
          <label for="title" class="col-md-2 control-label">Title</label>
          <div class="col-md-10">
            <input type="text" id="title" class="form-control" required />
          </div>
        </div>
        <div class="form-group">
          <label for="description" class="col-md-2 control-label">Description</label>
          <div class="col-md-10">
            <textarea id="description" class="form-control" rows="3"></textarea>
          </div>
        </div>
        <div class="form-group">
          <label for="itemsPerPage" class="col-md-2 control-label">Items Per Page</label>
          <div class="col-md-10">
            <input type="number" id="itemsPerPage" class="form-control" value="10" required />
          </div>
        </div>
        <div class="form-group">
          <label for="multipleContents TemplateId" class="col-md-2 control-label">Multiple Contents Template</label>
          <div class="col-md-10">
            <select id="multipleContentsTemplateId" class="form-control" required>
              ${options.multipleContentsTemplates.map(t => `<option value="${t.id}">${t.name}</option>`).join('')}
            </select>
          </div>
        </div>
        <div class="form-group">
          <label for="defaultSingleContentTemplateId" class="col-md-2 control-label">Default Single Content Template</label>
          <div class="col-md-10">
            <select id="defaultSingleContentTemplateId" class="form-control">
              ${options.singleContentTemplates.map(t => `<option value="${t.id}">${t.name}</option>`).join('')}
            </select>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-10 col-md-offset-2">
            <div class="checkbox">
              <label><input type="checkbox" id="isDefaultContentType" /> Is Default Content Type</label>
            </div>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <button type="submit" class="btn btn-primary">Create</button>
            <a href="/Admin/ContentTypes" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('createForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const defaultSingleId = parseInt(document.getElementById('defaultSingleContentTemplateId').value);

      const data = {
        name: document.getElementById('name').value,
        title: document.getElementById('title').value,
        description: document.getElementById('description').value,
        itemsPerPage: parseInt(document.getElementById('itemsPerPage').value),
        multipleContentsTemplateId: parseInt(document.getElementById('multipleContentsTemplateId').value),
        defaultSingleContentTemplateId: defaultSingleId > 0 ? defaultSingleId : null,
        isDefaultContentType: document.getElementById('isDefaultContentType').checked
      };

      try {
        const response = await fetch('/api/contenttypes', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.title || 'Failed to create content type');
        }

        window.location.href = '/Admin/ContentTypes';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading form: ${error.message}</div>`;
  }
});
