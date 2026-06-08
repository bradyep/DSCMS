// Layouts Details Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const layoutId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/layouts/${layoutId}`);
    if (!response.ok) throw new Error('Failed to load layout');

    const layout = await response.json();

    app.innerHTML = `
      <h2>Layout Details</h2>
      <dl class="dl-horizontal">
        <dt>Name</dt>
        <dd>${layout.name}</dd>
        <dt>Layout Source</dt>
        <dd>${layout.layoutSource}</dd>
        <dt>Source Type</dt>
        <dd>${layout.sourceTypeName || layout.sourceTypeId}</dd>
      </dl>
      <div>
        <a href="/Admin/Layouts/Edit/${layout.layoutId}" class="btn btn-primary">Edit</a>
        <a href="/Admin/Layouts" class="btn btn-default">Back to List</a>
      </div>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading layout: ${error.message}</div>`;
  }
});
