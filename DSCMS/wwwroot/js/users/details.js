// Users Details
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const id = window.location.pathname.split('/').pop();
  try {
    const response = await fetch(`/api/users/${id}`);
    const user = await response.json();
    app.innerHTML = `
      <h2>User Details</h2>
      <dl class="dl-horizontal">
        <dt>Display Name</dt><dd>${user.displayName || ''}</dd>
        <dt>Email</dt><dd>${user.email || ''}</dd>
        <dt>Username</dt><dd>${user.userName || ''}</dd>
      </dl>
      <div>
        <a href="/Admin/Users/Edit/${user.id}" class="btn btn-primary">Edit</a>
        <a href="/Admin/Users" class="btn btn-default">Back to List</a>
      </div>
    `;
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
