// Users Delete
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const id = window.location.pathname.split('/').pop();
  try {
    const response = await fetch(`/api/users/${id}`);
    const user = await response.json();
    app.innerHTML = `
      <h2>Delete User</h2>
      <h3>Are you sure you want to delete this?</h3>
      <dl class="dl-horizontal">
        <dt>Display Name</dt><dd>${user.displayName || ''}</dd>
        <dt>Email</dt><dd>${user.email || ''}</dd>
      </dl>
      <form id="deleteForm">
        <button type="submit" class="btn btn-danger">Delete</button>
        <a href="/Admin/Users" class="btn btn-default">Cancel</a>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;
    document.getElementById('deleteForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      const deleteResponse = await fetch(`/api/users/${id}`, { method: 'DELETE' });
      if (deleteResponse.ok) {
        window.location.href = '/Admin/Users';
      } else {
        document.getElementById('error').textContent = 'Failed to delete user';
        document.getElementById('error').style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
