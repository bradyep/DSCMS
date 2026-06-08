// ContentTypes Index Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');

  try {
    const response = await fetch('/api/contenttypes');
    if (!response.ok) throw new Error('Failed to load content types');

    const contentTypes = await response.json();

    app.innerHTML = `
      <h2>Content Types</h2>
      <p><a href="/Admin/ContentTypes/Create" class="btn btn-primary">Create New</a></p>
      <table class="table">
        <thead>
          <tr>
            <th>Name</th>
            <th>Title</th>
            <th>Description</th>
            <th>Items Per Page</th>
            <th>Multiple Contents Template</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          ${contentTypes.map(ct => `
            <tr>
              <td>${ct.name}</td>
              <td>${ct.title}</td>
              <td>${ct.description || ''}</td>
              <td>${ct.itemsPerPage}</td>
              <td>${ct.multipleContentsTemplateName || ''}</td>
              <td>
                <a href="/Admin/ContentTypes/Edit/${ct.contentTypeId}">Edit</a> |
                <a href="/Admin/ContentTypes/Details/${ct.contentTypeId}">Details</a> |
                <a href="/Admin/ContentTypes/Delete/${ct.contentTypeId}">Delete</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error loading content types: ${error.message}</div>`;
  }
});
