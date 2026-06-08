// Layouts Index Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    const response = await fetch('/api/layouts');
    if (!response.ok) throw new Error('Failed to load layouts');

    const layouts = await response.json();

    app.innerHTML = `
      <h2>Layouts</h2>
      <p><a href="/Admin/Layouts/Create" class="btn btn-primary">Create New</a></p>
      <table class="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Layout Source</th>
            <th>Source Type</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${layouts.map(layout => `
            <tr>
              <td>${layout.name || '[No Name]'}</td>
              <td>${layout.layoutSource || '[No Source]'}</td>
              <td>${layout.sourceTypeName || ''}</td>
              <td>
                <a href="/Admin/Layouts/Edit/${layout.layoutId}">Edit</a> |
                <a href="/Admin/Layouts/Details/${layout.layoutId}">Details</a> |
                <a href="/Admin/Layouts/Delete/${layout.layoutId}">Delete</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading layouts: ${error.message}</div>`;
  }
});
