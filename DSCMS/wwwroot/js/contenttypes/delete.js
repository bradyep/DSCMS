// ContentTypes Delete - displays data with delete confirmation
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const pathParts = window.location.pathname.split('/');
  const id = pathParts[pathParts.length - 1];

  try {
    const response = await fetch(`/api/contenttypes/${id}`);
    const ct = await response.json();

    app.innerHTML = `
      <h2>Delete Content Type</h2>
      <h3>Are you sure you want to delete this?</h3>
      <dl class="dl-horizontal">
        <dt>Name</dt><dd>${ct.name}</dd>
        <dt>Title</dt><dd>${ct.title}</dd>
        <dt>Description</dt><dd>${ct.description || ''}</dd>
      </dl>
      <form id="deleteForm">
        <button type="submit" class="btn btn-danger">Delete</button>
        <a href="/Admin/ContentTypes" class="btn btn-default">Cancel</a>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    document.getElementById('deleteForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      const deleteResponse = await fetch(`/api/contenttypes/${id}`, { method: 'DELETE' });
      if (deleteResponse.ok) {
        window.location.href = '/Admin/ContentTypes';
      } else {
        document.getElementById('error').textContent = 'Failed to delete';
        document.getElementById('error').style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
