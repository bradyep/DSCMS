// Templates Create Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    // Load form options
    const optionsResponse = await fetch('/api/templates/form-options');
    if (!optionsResponse.ok) throw new Error('Failed to load form options');
    const options = await optionsResponse.json();

    app.innerHTML = `
      <h2>Create Template</h2>
      <form id="createForm" class="form-horizontal">
        <div class="form-group">
          <label for="name" class="col-md-2 control-label">Name</label>
          <div class="col-md-10">
            <input type="text" id="name" name="name" class="form-control" />
          </div>
        </div>
        <div class="form-group">
          <label for="templateSource" class="col-md-2 control-label">Template Source</label>
          <div class="col-md-10">
            <input type="text" id="templateSource" name="templateSource" class="form-control" />
          </div>
        </div>
        <div class="form-group">
          <label for="sourceTypeId" class="col-md-2 control-label">Source Type ID</label>
          <div class="col-md-10">
            <input type="number" id="sourceTypeId" name="sourceTypeId" class="form-control" value="1" required />
            <small class="form-text text-muted">1 = RazorFile</small>
          </div>
        </div>
        <div class="form-group">
          <label for="isForMultipleContents" class="col-md-2 control-label">For Multiple Contents</label>
          <div class="col-md-10">
            <select id="isForMultipleContents" name="isForMultipleContents" class="form-control" required>
              <option value="0">No (Single Content)</option>
              <option value="1">Yes (Multiple Contents)</option>
            </select>
          </div>
        </div>
        <div class="form-group">
          <label for="layoutId" class="col-md-2 control-label">Layout</label>
          <div class="col-md-10">
            <select id="layoutId" name="layoutId" class="form-control">
              <option value="">-- None --</option>
              ${options.layouts.map(layout => `<option value="${layout.id}">${layout.name}</option>`).join('')}
            </select>
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <button type="submit" class="btn btn-primary">Create</button>
            <a href="/Admin/Templates" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('createForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const layoutIdValue = document.getElementById('layoutId').value;

      const data = {
        name: document.getElementById('name').value,
        templateSource: document.getElementById('templateSource').value,
        sourceTypeId: parseInt(document.getElementById('sourceTypeId').value),
        isForMultipleContents: parseInt(document.getElementById('isForMultipleContents').value),
        layoutId: layoutIdValue ? parseInt(layoutIdValue) : null
      };

      try {
        const response = await fetch('/api/templates', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!response.ok) {
          const errorData = await response.json();
          throw new Error(errorData.title || 'Failed to create template');
        }

        window.location.href = '/Admin/Templates';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading form: ${error.message}</div>`;
  }
});
