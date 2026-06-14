// Users Index Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  try {
    const response = await fetch('/api/users');
    const users = await response.json();
    app.innerHTML = `
      <h2>Users</h2>
      <p><a href="/Admin/Users/Create" class="btn btn-primary">Create New</a></p>
      <table class="table">
        <thead><tr><th>Display Name</th><th>Email</th><th>Username</th><th>Actions</th></tr></thead>
        <tbody>
          ${users.map(u => `
            <tr>
              <td>${u.displayName || ''}</td>
              <td>${u.email || ''}</td>
              <td>${u.userName || ''}</td>
              <td>
                <a href="/Admin/Users/Edit/${u.id}">Edit</a> |
                <a href="/Admin/Users/Details/${u.id}">Details</a> |
                <a href="/Admin/Users/Delete/${u.id}">Delete</a>
              </td>
            </tr>
          `).join('')}
        </tbody>
      </table>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
