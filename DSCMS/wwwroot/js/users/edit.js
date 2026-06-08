// Users Edit Page
document.addEventListener('DOMContentLoaded', async () => {
  const app = document.getElementById('app');
  const id = window.location.pathname.split('/').pop();

  try {
    const response = await fetch(`/api/users/${id}`);
    if (!response.ok) throw new Error('Failed to load user');

    const user = await response.json();

    app.innerHTML = `
      <h2>Edit User</h2>
      <form id="editForm" class="form-horizontal">
        <div class="form-group">
          <label for="displayName" class="col-md-2 control-label">Display Name</label>
          <div class="col-md-10">
            <input type="text" id="displayName" class="form-control" value="${user.displayName || ''}" />
          </div>
        </div>
        <div class="form-group">
          <label for="email" class="col-md-2 control-label">Email</label>
          <div class="col-md-10">
            <input type="email" id="email" class="form-control" value="${user.email || ''}" required />
          </div>
        </div>
        <div class="form-group">
          <label for="userName" class="col-md-2 control-label">Username</label>
          <div class="col-md-10">
            <input type="text" id="userName" class="form-control" value="${user.userName || ''}" />
          </div>
        </div>
        <div class="form-group">
          <div class="col-md-offset-2 col-md-10">
            <button type="submit" class="btn btn-primary">Save</button>
            <a href="/Admin/Users" class="btn btn-default">Cancel</a>
          </div>
        </div>
      </form>
      <div id="error" class="alert alert-danger" style="display:none;"></div>
    `;

    const form = document.getElementById('editForm');
    const errorDiv = document.getElementById('error');

    form.addEventListener('submit', async (e) => {
      e.preventDefault();

      const data = {
        displayName: document.getElementById('displayName').value,
        email: document.getElementById('email').value,
        userName: document.getElementById('userName').value
      };

      try {
        const updateResponse = await fetch(`/api/users/${id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(data)
        });

        if (!updateResponse.ok) {
          const errorData = await updateResponse.json();
          throw new Error(errorData.errors ? errorData.errors.join(', ') : 'Failed to update user');
        }

        window.location.href = '/Admin/Users';
      } catch (error) {
        errorDiv.textContent = `Error: ${error.message}`;
        errorDiv.style.display = 'block';
      }
    });
  } catch (error) {
    app.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`;
  }
});
