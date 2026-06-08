// Layouts Delete Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const layoutId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/layouts/${layoutId}`);
    if (!response.ok) throw new Error('Failed to load layout');

    const layout = await response.json();

    app.innerHTML = `
      <h2>Delete Layout</h2>
      <h3>Are you sure you want to delete this?</h3>
      <dl class="dl-horizontal">
        <dt>Name</dt>
        <dd>${layout.name}</dd>
        <dt>Layout Source</dt>
        <dd>${layout.layoutSource}</dd>
        <dt>Source Type</dt>
        <dd>${layout.sourceTypeName || layout.sourceTypeId}</dd>
      </dl>
      <form id="deleteForm">
        <button type="submit" class="btn btn-danger">Delete</button>
        <a href="/Admin/Layouts" class="btn btn-default">Cancel</a>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('deleteForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      try {
        const deleteResponse = await fetch(`/api/layouts/${layoutId}`, {
          method: 'DELETE'
        });

        if (!deleteResponse.ok) {
          throw new Error('Failed to delete layout');
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
