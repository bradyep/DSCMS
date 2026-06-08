// Templates Delete Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const templateId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/templates/${templateId}`);
    if (!response.ok) throw new Error('Failed to load template');

    const template = await response.json();

    app.innerHTML = `
      <h2>Delete Template</h2>
      <h3>Are you sure you want to delete this?</h3>
      <dl class="dl-horizontal">
        <dt>Name</dt>
        <dd>${template.name || '[No Name]'}</dd>
        <dt>Template Source</dt>
        <dd>${template.templateSource || '[No Source]'}</dd>
        <dt>For Multiple Contents</dt>
        <dd>${template.isForMultipleContents === 1 ? 'Yes' : 'No'}</dd>
        <dt>Layout</dt>
        <dd>${template.layoutName || '[No Layout]'}</dd>
      </dl>
      <form id="deleteForm">
        <button type="submit" class="btn btn-danger">Delete</button>
        <a href="/Admin/Templates" class="btn btn-default">Cancel</a>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('deleteForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      try {
        const deleteResponse = await fetch(`/api/templates/${templateId}`, {
          method: 'DELETE'
        });

        if (!deleteResponse.ok) {
          throw new Error('Failed to delete template');
        }

        window.location.href = '/Admin/Templates';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading template: ${error.message}</div>`;
  }
});
