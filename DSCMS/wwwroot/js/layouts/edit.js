// Layouts Edit Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const layoutId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/layouts/${layoutId}`);
    if (!response.ok) throw new Error('Failed to load layout');

    const layout = await response.json();

    app.innerHTML = `
      <h2>Edit Layout</h2>
      <form id="editForm" class="form-horizontal">
        <input type="hidden" id="layoutId" value="${layout.layoutId}" />
        <div class="form-group">
          <label for="name" class="col-md-2 control-label">Name</label>
          <div class="col-md-10">
            <input type="text" id="name" name="name" class="form-control" value="${layout.name}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="layoutSource" class="col-md-2 control-label">Layout Source</label>
          <div class="col-md-10">
            <input type="text" id="layoutSource" name="layoutSource" class="form-control" value="${layout.layoutSource}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="sourceTypeId" class="col-md-2 control-label">Source Type ID</label>
          <div class="col-md-10">
            <input type="number" id="sourceTypeId" name="sourceTypeId" class="form-control" value="${layout.sourceTypeId}" required />
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <button type="submit" class="btn btn-primary">Save</button>
            <a href="/Admin/Layouts" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('editForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const data = {
        name: document.getElementById('name').value,
        layoutSource: document.getElementById('layoutSource').value,
        sourceTypeId: parseInt(document.getElementById('sourceTypeId').value)
      };

      try {
        const updateResponse = await fetch(`/api/layouts/${layoutId}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!updateResponse.ok) {
          const errorData = await updateResponse.json();
          throw new Error(errorData.title || 'Failed to update layout');
        }

        window.location.href = '/Admin/Layouts';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading layout: ${error.message}</div>`;
  }
});
