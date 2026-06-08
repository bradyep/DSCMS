// Templates Details Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const templateId = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/templates/${templateId}`);
    if (!response.ok) throw new Error('Failed to load template');

    const template = await response.json();

    app.innerHTML = `
      <h2>Template Details</h2>
      <dl class="dl-horizontal">
        <dt>Name</dt>
        <dd>${template.name || '[No Name]'}</dd>
        <dt>Template Source</dt>
        <dd>${template.templateSource || '[No Source]'}</dd>
        <dt>Source Type ID</dt>
        <dd>${template.sourceTypeId}</dd>
        <dt>For Multiple Contents</dt>
        <dd>${template.isForMultipleContents === 1 ? 'Yes' : 'No'}</dd>
        <dt>Layout</dt>
        <dd>${template.layoutName || '[No Layout]'}</dd>
      </dl>
      <div>
        <a href="/Admin/Templates/Edit/${template.templateId}" class="btn btn-primary">Edit</a>
        <a href="/Admin/Templates" class="btn btn-default">Back to List</a>
      </div>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading template: ${error.message}</div>`;
  }
});
