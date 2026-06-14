// ContentTypes Details - displays readonly data
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const id = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/contenttypes/${id}`);
    const ct = await response.json();

    app.innerHTML = `
      <h2>Content Type Details</h2>
      <dl class="dl-horizontal">
        <dt>Name</dt><dd>${ct.name}</dd>
        <dt>Title</dt><dd>${ct.title}</dd>
        <dt>Description</dt><dd>${ct.description || ''}</dd>
        <dt>Items Per Page</dt><dd>${ct.itemsPerPage}</dd>
        <dt>Multiple Contents Template</dt><dd>${ct.multipleContentsTemplateName || ''}</dd>
        <dt>Default Single Content Template</dt><dd>${ct.defaultSingleContentTemplateName || '[None]'}</dd>
        <dt>Is Default</dt><dd>${ct.isDefaultContentType ? 'Yes' : 'No'}</dd>
      </dl>
      <div>
        <a href="/Admin/ContentTypes/Edit/${ct.contentTypeId}" class="btn btn-primary">Edit</a>
        <a href="/Admin/ContentTypes" class="btn btn-default">Back to List</a>
      </div>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
