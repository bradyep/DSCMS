// Templates Index Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    const response = await fetch('/api/templates');
    if (!response.ok) throw new Error('Failed to load templates');

    const templates = await response.json();

    app.innerHTML = `
      <h2>Templates</h2>
      <p><a href="/Admin/Templates/Create" class="btn btn-primary">Create New</a></p>
      <table class="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Template Source</th>
            <th>Layout</th>
            <th>For Multiple Contents</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${templates.map(template => `
            <tr>
              <td>${template.name || '[No Name]'}</td>
              <td>${template.templateSource || '[No Source]'}</td>
              <td>${template.layoutName || '[No Layout]'}</td>
              <td>${template.isForMultipleContents === 1 ? 'Yes' : 'No'}</td>
              <td>
                <a href="/Admin/Templates/Edit/${template.templateId}">Edit</a> |
                <a href="/Admin/Templates/Details/${template.templateId}">Details</a> |
                <a href="/Admin/Templates/Delete/${template.templateId}">Delete</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading templates: ${error.message}</div>`;
  }
});
