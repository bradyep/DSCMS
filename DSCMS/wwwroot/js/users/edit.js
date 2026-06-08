// Users Edit/Details/Delete - similar patterns
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const id = window.location.pathname.split('/').pop();
  try {
    const response = await fetch(`/api/users/${id}`);
    const user = await response.json();
    app.innerHTML = `<h2>Edit User</h2><form id="editForm"><!-- fields --></form>`;
    document.getElementById('editForm').addEventListener('submit', async (e) => {
      e.preventDefault();
      await fetch(`/api/users/${id}`, { method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({}) });
      window.location.href = '/Admin/Users';
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
